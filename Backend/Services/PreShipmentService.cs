// Author: Hassan
// Date: 2025-12-31
// Description: Service for Pre-Shipment operations - Manifest-based session creation before driver arrives

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Service implementation for Pre-Shipment operations
/// Allows warehouse staff to prepare shipments before driver arrives
/// </summary>
public class PreShipmentService : IPreShipmentService
{
    private readonly IShipmentLoadRepository _repository;
    private readonly IShipmentLoadService _shipmentLoadService;
    private readonly ILogger<PreShipmentService> _logger;

    public PreShipmentService(
        IShipmentLoadRepository repository,
        IShipmentLoadService shipmentLoadService,
        ILogger<PreShipmentService> logger)
    {
        _repository = repository;
        _shipmentLoadService = shipmentLoadService;
        _logger = logger;
    }

    /// <summary>
    /// Create Pre-Shipment session from manifest scan
    /// Flow:
    /// 1. Parse manifest barcode (44 bytes) → Extract Order Number
    /// 2. Query DB: Order Number → Route Number
    /// 3. Query DB: Route Number → ALL Orders + ALL Skids
    /// 4. Create ShipmentLoadSession with CreatedVia="PreShipment"
    /// 5. Return session ID, route, orders, planned skids
    /// </summary>
    public async Task<ApiResponse<CreateFromManifestResponseDto>> CreateFromManifestAsync(CreateFromManifestRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Creating session from manifest: {Manifest}", request.ManifestBarcode);

            // 1. Parse manifest barcode (44 bytes)
            // Format: Plant(5) + Supplier(5) + Dock(2) + Order(12) + LoadId(12) + PalletCode(2) + MROS(2) + SkidId(4)
            // Example: 26MTM05474FB2025121134  JFB34       A434001A
            //          |    |    | |           |           |  | |
            //          0    5   10 12          24          36 38 40
            if (request.ManifestBarcode.Length < 44)
            {
                return ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "Invalid manifest barcode",
                    $"Manifest barcode must be 44 bytes. Received: {request.ManifestBarcode.Length} bytes");
            }

            var plantCode = request.ManifestBarcode.Substring(0, 5).Trim();           // 0-5: Plant (5 chars)
            var supplierCode = request.ManifestBarcode.Substring(5, 5).Trim();        // 5-10: Supplier (5 chars)
            var dockCode = request.ManifestBarcode.Substring(10, 2).Trim();           // 10-12: Dock (2 chars)
            var orderNumber = request.ManifestBarcode.Substring(12, 12).Trim();       // 12-24: Order (12 chars)
            var loadId = request.ManifestBarcode.Substring(24, 12).Trim();            // 24-36: LoadId (12 chars)
            var palletizationCode = request.ManifestBarcode.Substring(36, 2);         // 36-38: Palletization Code (2 chars)
            var mros = request.ManifestBarcode.Substring(38, 2);                      // 38-40: MROS (2 chars)
            var skidId = request.ManifestBarcode.Substring(40, 4);                    // 40-44: SkidId (4 chars)

            _logger.LogInformation("[PRE-SHIPMENT] Parsed manifest - Plant: {Plant}, Supplier: {Supplier}, Dock: {Dock}, Order: {Order}, LoadId: {LoadId}, PalletCode: {PalletCode}, MROS: {MROS}, SkidId: {SkidId}",
                plantCode, supplierCode, dockCode, orderNumber, loadId, palletizationCode, mros, skidId);

            // 2. Query DB: Order Number + Dock → Route Number
            var routeNumber = await _repository.GetRouteByOrderNumberAsync(orderNumber, dockCode);

            if (string.IsNullOrEmpty(routeNumber))
            {
                return ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with OrderNumber '{orderNumber}' and DockCode '{dockCode}'. Cannot determine route.");
            }

            _logger.LogInformation("[PRE-SHIPMENT] Route determined from order {Order}-{Dock}: {Route}",
                orderNumber, dockCode, routeNumber);

            // 3. Check if Pre-Shipment session already exists for this route
            var existingSession = await _repository.GetSessionByRouteAndCreatedViaAsync(routeNumber, "PreShipment");

            if (existingSession != null)
            {
                _logger.LogInformation("[PRE-SHIPMENT] Resuming existing session: {SessionId}", existingSession.SessionId);

                // Return existing session details
                var existingOrders = await _repository.GetOrdersByRouteAsync(routeNumber);
                var plannedSkids = await GetPlannedSkidsAsync(existingOrders, existingSession.SessionId);

                return ApiResponse<CreateFromManifestResponseDto>.SuccessResponse(
                    MapToCreateFromManifestResponse(existingSession, existingOrders, plannedSkids),
                    "Pre-Shipment session already exists for this route. Resuming existing session.");
            }

            // 4. Get ALL orders on this route
            var orders = await _repository.GetOrdersByRouteAsync(routeNumber);

            if (!orders.Any())
            {
                return ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "No orders on route",
                    $"No orders found for route '{routeNumber}'. Cannot create Pre-Shipment session.");
            }

            // 5. Validate all orders are ready (Status >= SkidBuilt)
            var ordersNotReady = orders
                .Where(o => o.Status < OrderStatus.SkidBuilt)
                .Select(o => $"{o.RealOrderNumber} (Status: {o.Status})")
                .ToList();

            if (ordersNotReady.Any())
            {
                return ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "Orders not ready",
                    $"The following orders have not completed skid build: {string.Join(", ", ordersNotReady)}. Complete skid build first.");
            }

            // 6. Parse route and run
            var (route, run) = ParseRouteAndRun(routeNumber);

            // 7. Create ShipmentLoadSession with CreatedVia="PreShipment"
            var session = new ShipmentLoadSession
            {
                SessionId = Guid.NewGuid(),
                RouteNumber = routeNumber,
                Run = run,
                UserId = request.ScannedBy,
                SupplierCode = supplierCode,
                PickupDateTime = null, // Will be set when completing
                Status = "active",
                CreatedVia = "PreShipment", // CRITICAL: Mark as Pre-Shipment
                CreatedAt = DateTime.Now,
                CreatedBy = request.ScannedBy.ToString()
            };

            session = await _repository.CreateSessionAsync(session);

            _logger.LogInformation("[PRE-SHIPMENT] Created session: {SessionId} for route {Route} with {OrderCount} orders",
                session.SessionId, routeNumber, orders.Count);

            // 8. Get all planned skids for this route
            var allPlannedSkids = await GetPlannedSkidsAsync(orders, session.SessionId);

            // 9. Build response
            var response = MapToCreateFromManifestResponse(session, orders, allPlannedSkids);

            return ApiResponse<CreateFromManifestResponseDto>.SuccessResponse(
                response,
                $"Pre-Shipment session created successfully for route {routeNumber}. {orders.Count} orders, {allPlannedSkids.Count} planned skids.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error creating session from manifest: {Manifest}", request.ManifestBarcode);
            return ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                "Failed to create Pre-Shipment session",
                ex.Message);
        }
    }

    /// <summary>
    /// Get list of all Pre-Shipment sessions
    /// </summary>
    public async Task<ApiResponse<List<PreShipmentListItemDto>>> GetListAsync()
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Getting list of Pre-Shipment sessions");

            // Get all sessions where CreatedVia = "PreShipment"
            var sessions = await _repository.GetSessionsByCreatedViaAsync("PreShipment");

            var result = new List<PreShipmentListItemDto>();

            foreach (var session in sessions)
            {
                // Get all orders on this route
                var orders = await _repository.GetOrdersByRouteAsync(session.RouteNumber);

                // Calculate total skids and scanned skids
                var totalSkidCount = 0;
                var scannedSkidCount = 0;

                foreach (var order in orders)
                {
                    var skidScans = await _repository.GetSkidScansByOrderIdAsync(order.OrderId);
                    totalSkidCount += skidScans.Count;

                    // FIXED: Count individual skids that have ShipmentLoadSessionId set
                    // Each skid is tracked individually, not at order level
                    foreach (var scan in skidScans)
                    {
                        if (scan.ShipmentLoadSessionId == session.SessionId)
                        {
                            scannedSkidCount++;
                        }
                    }
                }

                result.Add(new PreShipmentListItemDto
                {
                    SessionId = session.SessionId,
                    RouteNumber = session.RouteNumber,
                    SupplierCode = session.SupplierCode,
                    Status = session.Status ?? "active",
                    TotalSkidCount = totalSkidCount,
                    ScannedSkidCount = scannedSkidCount,
                    CreatedAt = session.CreatedAt,
                    TrailerNumber = session.TrailerNumber,
                    CreatedBy = session.CreatedBy,
                    ToyotaStatus = session.ToyotaStatus,
                    ToyotaConfirmationNumber = session.ToyotaConfirmationNumber
                });
            }

            return ApiResponse<List<PreShipmentListItemDto>>.SuccessResponse(
                result,
                $"Retrieved {result.Count} Pre-Shipment sessions successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error getting session list");
            return ApiResponse<List<PreShipmentListItemDto>>.ErrorResponse(
                "Failed to retrieve Pre-Shipment sessions",
                ex.Message);
        }
    }

    /// <summary>
    /// Get Pre-Shipment session details by ID
    /// Reuses ShipmentLoadService.GetSessionAsync
    /// </summary>
    public async Task<ApiResponse<SessionResponseDto>> GetSessionAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Getting session: {SessionId}", sessionId);

            // Verify it's a Pre-Shipment session
            var session = await _repository.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {sessionId}");
            }

            if (session.CreatedVia != "PreShipment")
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Not a Pre-Shipment session",
                    $"Session {sessionId} was created via '{session.CreatedVia}', not 'PreShipment'");
            }

            // Reuse ShipmentLoadService
            return await _shipmentLoadService.GetSessionAsync(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error getting session: {SessionId}", sessionId);
            return ApiResponse<SessionResponseDto>.ErrorResponse(
                "Failed to retrieve Pre-Shipment session",
                ex.Message);
        }
    }

    /// <summary>
    /// Update Pre-Shipment session with trailer/driver info
    /// Reuses ShipmentLoadService.UpdateSessionAsync
    /// </summary>
    public async Task<ApiResponse<SessionResponseDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Updating session: {SessionId}", sessionId);

            // Verify it's a Pre-Shipment session
            var session = await _repository.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {sessionId}");
            }

            if (session.CreatedVia != "PreShipment")
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Not a Pre-Shipment session",
                    $"Session {sessionId} was created via '{session.CreatedVia}', not 'PreShipment'");
            }

            // Reuse ShipmentLoadService
            return await _shipmentLoadService.UpdateSessionAsync(sessionId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error updating session: {SessionId}", sessionId);
            return ApiResponse<SessionResponseDto>.ErrorResponse(
                "Failed to update Pre-Shipment session",
                ex.Message);
        }
    }

    /// <summary>
    /// Complete Pre-Shipment session and submit to Toyota API
    /// Reuses ShipmentLoadService.CompleteShipmentAsync
    /// </summary>
    public async Task<ApiResponse<PreShipmentCompleteResponseDto>> CompleteAsync(PreShipmentCompleteRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Completing session: {SessionId}", request.SessionId);

            // Verify it's a Pre-Shipment session
            var session = await _repository.GetSessionByIdAsync(request.SessionId);
            if (session == null)
            {
                return ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {request.SessionId}");
            }

            if (session.CreatedVia != "PreShipment")
            {
                return ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                    "Not a Pre-Shipment session",
                    $"Session {request.SessionId} was created via '{session.CreatedVia}', not 'PreShipment'");
            }

            // Reuse ShipmentLoadService.CompleteShipmentAsync
            var shipmentLoadRequest = new ShipmentLoadCompleteRequestDto
            {
                SessionId = request.SessionId,
                UserId = request.UserId
            };

            var shipmentLoadResult = await _shipmentLoadService.CompleteShipmentAsync(shipmentLoadRequest);

            if (!shipmentLoadResult.Success)
            {
                return ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                    shipmentLoadResult.Message ?? "Failed to complete Pre-Shipment",
                    shipmentLoadResult.Errors?.FirstOrDefault() ?? "Unknown error");
            }

            // Map to PreShipmentCompleteResponseDto
            var response = new PreShipmentCompleteResponseDto
            {
                ConfirmationNumber = shipmentLoadResult.Data!.ConfirmationNumber,
                RouteNumber = shipmentLoadResult.Data.RouteNumber,
                TrailerNumber = shipmentLoadResult.Data.TrailerNumber,
                TotalOrdersShipped = shipmentLoadResult.Data.TotalOrdersShipped,
                TotalSkidsShipped = 0, // Would need to calculate from orders
                CompletedAt = shipmentLoadResult.Data.CompletedAt,
                ShippedOrderNumbers = shipmentLoadResult.Data.ShippedOrderNumbers
            };

            return ApiResponse<PreShipmentCompleteResponseDto>.SuccessResponse(
                response,
                $"Pre-Shipment completed successfully. Toyota Confirmation: {response.ConfirmationNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error completing session: {SessionId}", request.SessionId);
            return ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                "Failed to complete Pre-Shipment session",
                ex.Message);
        }
    }

    /// <summary>
    /// Delete incomplete Pre-Shipment session
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Deleting session: {SessionId}", sessionId);

            var session = await _repository.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {sessionId}");
            }

            if (session.CreatedVia != "PreShipment")
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Not a Pre-Shipment session",
                    $"Session {sessionId} was created via '{session.CreatedVia}', not 'PreShipment'");
            }

            if (session.Status == "completed")
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Cannot delete completed session",
                    "Completed Pre-Shipment sessions cannot be deleted");
            }

            // Mark as cancelled instead of deleting
            session.Status = "cancelled";
            session.UpdatedAt = DateTime.Now;
            await _repository.UpdateSessionAsync(session);

            _logger.LogInformation("[PRE-SHIPMENT] Session cancelled: {SessionId}", sessionId);

            return ApiResponse<bool>.SuccessResponse(
                true,
                "Pre-Shipment session cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error deleting session: {SessionId}", sessionId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete Pre-Shipment session",
                ex.Message);
        }
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Get all planned skids for a list of orders
    /// FIXED: IsScanned checks if skid's ShipmentLoadSessionId matches CURRENT session
    /// </summary>
    private async Task<List<PlannedSkidDto>> GetPlannedSkidsAsync(List<Order> orders, Guid sessionId)
    {
        var plannedSkids = new List<PlannedSkidDto>();

        foreach (var order in orders)
        {
            var skidScans = await _repository.GetSkidScansByOrderIdAsync(order.OrderId);

            foreach (var scan in skidScans)
            {
                var rawSkidId = scan.RawSkidId ?? scan.SkidNumber.ToString();

                // Extract SkidNumber (first 3 chars) and SkidSide (4th char)
                var skidNumber = rawSkidId.Length >= 3 ? rawSkidId.Substring(0, 3) : scan.SkidNumber.ToString().PadLeft(3, '0');
                var skidSide = rawSkidId.Length >= 4 ? rawSkidId.Substring(3, 1) : null;

                plannedSkids.Add(new PlannedSkidDto
                {
                    OrderNumber = order.RealOrderNumber,
                    DockCode = order.DockCode,
                    SkidId = rawSkidId,
                    SkidNumber = skidNumber,
                    SkidSide = skidSide,
                    PalletizationCode = scan.PalletizationCode,
                    PartCount = 1, // Default to 1 part per skid (actual count tracked in PlannedItem)
                    IsScanned = scan.ShipmentLoadSessionId == sessionId // FIXED: Check if scanned in CURRENT session
                });
            }
        }

        return plannedSkids;
    }

    /// <summary>
    /// Parse route and run from RouteNumber
    /// Example: "YUAN03" -> Route: "YUAN", Run: "03"
    /// Example: "JAAJ-17" -> Route: "JAAJ", Run: "17"
    /// </summary>
    private (string route, string run) ParseRouteAndRun(string routeNumber)
    {
        if (string.IsNullOrEmpty(routeNumber) || routeNumber.Length < 2)
            return (routeNumber, "");

        // Run is last 2 characters, Route is everything before
        var run = routeNumber.Substring(routeNumber.Length - 2);
        var route = routeNumber.Substring(0, routeNumber.Length - 2);

        // Strip trailing hyphen from route (e.g., "JAAJ-17" -> route="JAAJ", run="17")
        route = route.TrimEnd('-');

        return (route, run);
    }

    /// <summary>
    /// Map session, orders, and skids to CreateFromManifestResponseDto
    /// </summary>
    private CreateFromManifestResponseDto MapToCreateFromManifestResponse(
        ShipmentLoadSession session,
        List<Order> orders,
        List<PlannedSkidDto> plannedSkids)
    {
        var (route, run) = ParseRouteAndRun(session.RouteNumber);

        var orderDtos = orders.Select(o => new ShipmentLoadOrderDto
        {
            OrderId = o.OrderId,
            OrderNumber = o.RealOrderNumber,
            DockCode = o.DockCode,
            SupplierCode = o.SupplierCode,
            PlantCode = o.PlantCode,
            PlannedRoute = o.PlannedRoute,
            Status = o.Status.ToString(),
            TotalSkids = plannedSkids.Count(s => s.OrderNumber == o.RealOrderNumber),
            IsScanned = o.ShipmentLoadSessionId == session.SessionId
        }).ToList();

        return new CreateFromManifestResponseDto
        {
            SessionId = session.SessionId,
            RouteNumber = session.RouteNumber,
            Route = route,
            Run = run,
            SupplierCode = session.SupplierCode ?? "",
            Orders = orderDtos,
            PlannedSkids = plannedSkids,
            TotalOrders = orders.Count,
            TotalSkids = plannedSkids.Count,
            CreatedAt = session.CreatedAt
        };
    }
}
