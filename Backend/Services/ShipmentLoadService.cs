// Author: Hassan
// Date: 2025-12-17
// Updated: 2025-12-22 - Fixed Order.Status to set ShipmentError when Toyota API fails
// Updated: 2025-12-22 - Fixed IsScanned logic to only check current session (removed status check)
// Updated: 2025-12-24 - Fixed skid build exceptions not being included in Toyota shipment load payload
// Updated: 2025-12-24 - Fixed exception code mapping: Skid Build code 12 -> Shipment Load code 24 at trailer level
// Updated: 2026-01-04 - Fixed Toyota duplicate skid issue: Group by (PalletizationCode + RawSkidId) to send one skid per manifest
// Description: Service for Shipment Load operations - Toyota SCS integration with session management

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Shipment Load service operations
/// </summary>
public interface IShipmentLoadService
{
    // Session operations
    Task<ApiResponse<SessionResponseDto>> StartOrResumeSessionAsync(StartSessionRequestDto request);
    Task<ApiResponse<SessionResponseDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequestDto request);
    Task<ApiResponse<SessionResponseDto>> GetSessionAsync(Guid sessionId);

    // Scan operations
    Task<ApiResponse<ShipmentLoadScanResponseDto>> ValidateAndScanOrderAsync(ShipmentLoadScanRequestDto request);

    // Validation operations (without session)
    Task<ApiResponse<ValidateOrderResponseDto>> ValidateOrderAsync(string orderNumber, string dockCode);

    // Exception operations
    Task<ApiResponse<ExceptionDto>> AddExceptionAsync(AddExceptionRequestDto request);
    Task<ApiResponse<bool>> RemoveExceptionAsync(Guid exceptionId);

    // Completion operations
    Task<ApiResponse<ShipmentLoadCompleteResponseDto>> CompleteShipmentAsync(ShipmentLoadCompleteRequestDto request);

    // Legacy operations (for backwards compatibility)
    Task<ApiResponse<ShipmentLoadRouteResponseDto>> GetOrdersByRouteAsync(string routeNumber);
}

/// <summary>
/// Service implementation for Shipment Load operations
/// </summary>
public class ShipmentLoadService : IShipmentLoadService
{
    private readonly IShipmentLoadRepository _repository;
    private readonly IToyotaApiService _toyotaApiService;
    private readonly IToyotaValidationService _toyotaValidationService;
    private readonly ILogger<ShipmentLoadService> _logger;

    // System user ID for operations when user is not authenticated
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ShipmentLoadService(
        IShipmentLoadRepository repository,
        IToyotaApiService toyotaApiService,
        IToyotaValidationService toyotaValidationService,
        ILogger<ShipmentLoadService> logger)
    {
        _repository = repository;
        _toyotaApiService = toyotaApiService;
        _toyotaValidationService = toyotaValidationService;
        _logger = logger;
    }

    // ===== SESSION OPERATIONS =====

    /// <summary>
    /// Start a new session or resume existing active session for a route
    /// </summary>
    public async Task<ApiResponse<SessionResponseDto>> StartOrResumeSessionAsync(StartSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting/resuming session for route: {RouteNumber}", request.RouteNumber);

            int? scannedOrderSkidCount = null;

            // If orderNumber and dockCode are provided, validate the order
            if (!string.IsNullOrEmpty(request.OrderNumber) && !string.IsNullOrEmpty(request.DockCode))
            {
                var order = await _repository.GetOrderByNumberAndDockAsync(request.OrderNumber, request.DockCode);

                if (order == null)
                {
                    return ApiResponse<SessionResponseDto>.ErrorResponse(
                        "Order not found",
                        $"No order found with OrderNumber '{request.OrderNumber}' and DockCode '{request.DockCode}'");
                }

                // Verify order status >= SkidBuilt (skid-build completed)
                if (order.Status < OrderStatus.SkidBuilt)
                {
                    return ApiResponse<SessionResponseDto>.ErrorResponse(
                        "Order not ready",
                        $"Order {request.OrderNumber} has not completed skid-build yet (Status: {order.Status}). Required status: SkidBuilt or higher.");
                }

                // Get the actual skid count from tblSkidScans for this order
                scannedOrderSkidCount = await _repository.GetSkidScansCountForOrderAsync(order.OrderId);

                _logger.LogInformation("Order {OrderNumber}-{DockCode} validated. Status: {Status}, Skid Count: {SkidCount}",
                    request.OrderNumber, request.DockCode, order.Status, scannedOrderSkidCount);
            }

            // ===== VALIDATE ALL ORDERS ON ROUTE HAVE COMPLETED SKID BUILD =====
            // Get ALL orders on this route (regardless of status) to check if any are not ready
            var allOrdersOnRoute = await _repository.GetAllOrdersByRouteAsync(request.RouteNumber);

            if (allOrdersOnRoute.Count == 0)
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "No orders found",
                    $"No orders found for route '{request.RouteNumber}'. Please verify the route number.");
            }

            // Check for orders that haven't completed skid build
            var ordersNotReady = allOrdersOnRoute
                .Where(o => o.Status < OrderStatus.SkidBuilt)
                .Select(o => $"{o.RealOrderNumber} (Status: {o.Status})")
                .ToList();

            if (ordersNotReady.Any())
            {
                _logger.LogWarning("Cannot start shipment for route {RouteNumber} - {Count} orders not ready: {Orders}",
                    request.RouteNumber, ordersNotReady.Count, string.Join(", ", ordersNotReady));

                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Orders not ready for shipment",
                    $"The following orders have not completed skid build: {string.Join(", ", ordersNotReady)}. All orders on the route must complete skid build before starting shipment.");
            }

            _logger.LogInformation("All {Count} orders on route {RouteNumber} are ready for shipment",
                allOrdersOnRoute.Count, request.RouteNumber);

            // Check if active session exists for this route
            // IMPORTANT: First check for Pre-Shipment session, then Shipment Load session
            var preShipmentSession = await _repository.GetSessionByRouteAndCreatedViaAsync(request.RouteNumber, "PreShipment");
            var existingSession = preShipmentSession ?? await _repository.GetActiveSessionByRouteAsync(request.RouteNumber);

            bool isResumed = false;

            if (existingSession != null)
            {
                _logger.LogInformation("Resuming existing session: {SessionId}, Status: {Status}, CreatedVia: {CreatedVia}",
                    existingSession.SessionId, existingSession.Status, existingSession.CreatedVia);
                isResumed = true;

                // CRITICAL FIX: Update PickupDateTime from request when resuming PreShipment session
                // PreShipment sessions are created without PickupDateTime (from manifest scan)
                // ShipmentLoad provides PickupDateTime from Pickup Route QR scan
                bool needsUpdate = false;

                if (existingSession.PickupDateTime == null && request.PickupDateTime != null)
                {
                    _logger.LogInformation("Updating PickupDateTime for PreShipment session {SessionId}: {PickupDateTime}",
                        existingSession.SessionId, request.PickupDateTime);
                    existingSession.PickupDateTime = request.PickupDateTime;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    existingSession.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateSessionAsync(existingSession);
                }
            }
            else
            {
                // Parse route and run from RouteNumber (e.g., "YUAN03" -> Route: "YUAN", Run: "03")
                var (route, run) = ParseRouteAndRun(request.RouteNumber);

                // Create new session
                existingSession = new ShipmentLoadSession
                {
                    SessionId = Guid.NewGuid(),
                    RouteNumber = request.RouteNumber,
                    Run = run,
                    UserId = request.UserId,
                    SupplierCode = request.SupplierCode,
                    PickupDateTime = request.PickupDateTime,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId.ToString()
                };

                existingSession = await _repository.CreateSessionAsync(existingSession);
                _logger.LogInformation("Created new session: {SessionId}", existingSession.SessionId);
            }

            // Get ALL orders for this route that are ready to ship (Status >= SkidBuilt)
            // This ensures we show ALL orders on the route, not just ones already scanned
            var orders = await _repository.GetOrdersByRouteAsync(existingSession.RouteNumber);
            _logger.LogInformation("GetOrdersByRouteAsync returned {Count} orders for route {Route}: {OrderNumbers}",
                orders.Count, existingSession.RouteNumber,
                string.Join(", ", orders.Select(o => $"{o.RealOrderNumber}-{o.DockCode}")));

            // Get exceptions for this session
            var exceptions = await _repository.GetSessionExceptionsAsync(existingSession.SessionId);

            // Map to response DTO
            var response = await MapSessionToDtoAsync(existingSession, orders, exceptions, isResumed);
            response.ScannedOrderSkidCount = scannedOrderSkidCount;

            return ApiResponse<SessionResponseDto>.SuccessResponse(
                response,
                isResumed ? "Session resumed successfully" : "Session started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting/resuming session for route: {RouteNumber}", request.RouteNumber);
            return ApiResponse<SessionResponseDto>.ErrorResponse(
                "Failed to start session",
                ex.Message);
        }
    }

    /// <summary>
    /// Update session with trailer information
    /// </summary>
    public async Task<ApiResponse<SessionResponseDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequestDto request)
    {
        try
        {
            var session = await _repository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No active session found with ID: {sessionId}");
            }

            // Update session fields
            session.TrailerNumber = request.TrailerNumber;
            session.SealNumber = request.SealNumber;
            session.LpCode = request.LpCode;
            session.DriverFirstName = request.DriverFirstName;
            session.DriverLastName = request.DriverLastName;
            session.SupplierFirstName = request.SupplierFirstName;
            session.SupplierLastName = request.SupplierLastName;
            session.UpdatedAt = DateTime.UtcNow;

            session = await _repository.UpdateSessionAsync(session);

            // Get ALL orders for this route (not just linked ones) to show planned orders
            var orders = await _repository.GetOrdersByRouteAsync(session.RouteNumber);
            var exceptions = await _repository.GetSessionExceptionsAsync(sessionId);
            var response = await MapSessionToDtoAsync(session, orders, exceptions, false);

            _logger.LogInformation("Session updated: {SessionId}", sessionId);

            return ApiResponse<SessionResponseDto>.SuccessResponse(
                response,
                "Session updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session: {SessionId}", sessionId);
            return ApiResponse<SessionResponseDto>.ErrorResponse(
                "Failed to update session",
                ex.Message);
        }
    }

    /// <summary>
    /// Get session with orders and exceptions
    /// </summary>
    public async Task<ApiResponse<SessionResponseDto>> GetSessionAsync(Guid sessionId)
    {
        try
        {
            var session = await _repository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {sessionId}");
            }

            // Get ALL orders for this route (not just linked ones) to show planned orders
            var orders = await _repository.GetOrdersByRouteAsync(session.RouteNumber);
            var exceptions = await _repository.GetSessionExceptionsAsync(sessionId);
            var response = await MapSessionToDtoAsync(session, orders, exceptions, false);

            return ApiResponse<SessionResponseDto>.SuccessResponse(
                response,
                "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            return ApiResponse<SessionResponseDto>.ErrorResponse(
                "Failed to retrieve session",
                ex.Message);
        }
    }

    // ===== SCAN OPERATIONS =====

    /// <summary>
    /// Validate and scan order for shipment loading
    /// Links order to session
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadScanResponseDto>> ValidateAndScanOrderAsync(ShipmentLoadScanRequestDto request)
    {
        try
        {
            // 1. Validate session exists
            var session = await _repository.GetSessionByIdAsync(request.SessionId);
            if (session == null)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No active session found with ID: {request.SessionId}");
            }

            // 2. Check if order exists
            var order = await _repository.GetOrderByNumberAndDockAsync(
                request.OrderNumber, request.DockCode);

            if (order == null)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with number {request.OrderNumber} and dock code {request.DockCode}");
            }

            // 3. Check if order status is >= SkidBuilt
            if (order.Status < OrderStatus.SkidBuilt)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Order not ready",
                    $"Order {request.OrderNumber} has not been built yet (Status: {order.Status})");
            }

            // 4. Route validation - Accept BOTH Main Route and Sub-Route
            // Pickup QR uses SUB-ROUTE (e.g., JAAJ17)
            // Manifest uses MAIN ROUTE (e.g., JFB34)
            // Both are valid for the same delivery
            //
            // The scan request contains routeNumber from the manifest (MAIN ROUTE)
            // The session has routeNumber from pickup QR (SUB-ROUTE)
            // Accept if either:
            // - Order's PlannedRoute matches session route (SUB-ROUTE)
            // - Order's PlannedRoute matches the route from scan request (MAIN ROUTE)
            // - Or if order belongs to same supplier/dock combination (already validated by OrderNumber + DockCode match)

            // Since we already validated the order exists and matches by OrderNumber + DockCode,
            // and we validated the session exists, the route check is redundant.
            // The OrderNumber + DockCode combination is the true identifier.
            // Route is informational only.

            _logger.LogInformation("Route info - Session: {SessionRoute}, Order PlannedRoute: {PlannedRoute}",
                session.RouteNumber, order.PlannedRoute);

            // 5. Check if order is already shipped
            if (order.Status == OrderStatus.Shipped)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Already shipped",
                    $"Order {request.OrderNumber} has already been shipped");
            }

            // 6. Verify skid scans exist (proves skid was built)
            var skidScansCount = await _repository.GetSkidScansCountForOrderAsync(order.OrderId);
            if (skidScansCount == 0)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "No skid scans found",
                    $"Order {request.OrderNumber} has no skid build scans. Complete Skid Build first.");
            }

            // All validations passed - link order to session
            await _repository.LinkOrderToSessionAsync(order.OrderId, request.SessionId);

            // Update order status to ShipmentLoading
            order.Status = OrderStatus.ShipmentLoading;
            order.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateOrdersAsync(new List<Order> { order });

            // FIXED: If SkidId is provided, mark the individual skid as scanned
            // CRITICAL: Pass PalletizationCode to help identify the correct skid
            // Within same order, RawSkidId + PalletizationCode should uniquely identify the skid
            if (!string.IsNullOrWhiteSpace(request.SkidId))
            {
                var skidScan = await _repository.GetSkidScanByRawSkidIdAsync(
                    order.OrderId,
                    request.SkidId,
                    request.PalletizationCode);

                if (skidScan != null)
                {
                    skidScan.ShipmentLoadSessionId = request.SessionId;
                    await _repository.UpdateSkidScanAsync(skidScan);
                    _logger.LogInformation("Marked individual skid {SkidId} (Pallet: {PalletizationCode}) as scanned for session {SessionId}",
                        request.SkidId, request.PalletizationCode ?? "NULL", request.SessionId);
                }
                else
                {
                    _logger.LogWarning("SkidId {SkidId} (Pallet: {PalletizationCode}) not found for order {OrderNumber}-{DockCode}",
                        request.SkidId, request.PalletizationCode ?? "NULL", request.OrderNumber, request.DockCode);
                }
            }

            var response = new ShipmentLoadScanResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                Status = order.Status.ToString(),
                ValidationMessage = $"Order {request.OrderNumber} validated successfully. {skidScansCount} skid(s) confirmed.",
                ScannedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Order scanned for shipment: {OrderNumber}-{DockCode} linked to session {SessionId}",
                request.OrderNumber, request.DockCode, request.SessionId);

            return ApiResponse<ShipmentLoadScanResponseDto>.SuccessResponse(
                response,
                $"Order {request.OrderNumber} scanned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating and scanning order: {OrderNumber}-{DockCode}",
                request.OrderNumber, request.DockCode);
            return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                "Failed to scan order",
                ex.Message);
        }
    }

    // ===== EXCEPTION OPERATIONS =====

    /// <summary>
    /// Add exception to session (trailer-level or skid-level)
    /// </summary>
    public async Task<ApiResponse<ExceptionDto>> AddExceptionAsync(AddExceptionRequestDto request)
    {
        try
        {
            var exception = new ShipmentLoadException
            {
                ExceptionId = Guid.NewGuid(),
                SessionId = request.SessionId,
                ExceptionType = request.ExceptionType,
                Comments = request.Comments,
                RelatedSkidId = request.RelatedSkidId,
                CreatedByUser = request.CreatedByUserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            exception = await _repository.AddExceptionAsync(exception);

            var response = new ExceptionDto
            {
                ExceptionId = exception.ExceptionId,
                ExceptionType = exception.ExceptionType!,
                Comments = exception.Comments,
                RelatedSkidId = exception.RelatedSkidId,
                CreatedAt = exception.CreatedAt
            };

            return ApiResponse<ExceptionDto>.SuccessResponse(
                response,
                "Exception added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exception to session: {SessionId}", request.SessionId);
            return ApiResponse<ExceptionDto>.ErrorResponse(
                "Failed to add exception",
                ex.Message);
        }
    }

    /// <summary>
    /// Remove exception from session
    /// </summary>
    public async Task<ApiResponse<bool>> RemoveExceptionAsync(Guid exceptionId)
    {
        try
        {
            await _repository.DeleteExceptionAsync(exceptionId);
            return ApiResponse<bool>.SuccessResponse(true, "Exception removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing exception: {ExceptionId}", exceptionId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to remove exception",
                ex.Message);
        }
    }

    // ===== COMPLETION OPERATIONS =====

    /// <summary>
    /// Complete shipment - builds Toyota payload and submits to Toyota API
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadCompleteResponseDto>> CompleteShipmentAsync(ShipmentLoadCompleteRequestDto request)
    {
        try
        {
            _logger.LogInformation("Completing shipment for session: {SessionId}", request.SessionId);

            // 1. Get session with all data
            var session = await _repository.GetSessionByIdAsync(request.SessionId);
            if (session == null)
            {
                return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Session not found",
                    $"No session found with ID: {request.SessionId}");
            }

            // 2. Get all orders linked to this session
            var orders = await _repository.GetOrdersBySessionIdAsync(request.SessionId);
            if (!orders.Any())
            {
                return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "No orders to ship",
                    "No orders have been scanned for this session");
            }

            // 3. Get exceptions
            var exceptions = await _repository.GetSessionExceptionsAsync(request.SessionId);

            // GAP-002: Validate driver names when dropHook=false (VUTEQ always has dropHook=false)
            if (string.IsNullOrWhiteSpace(session.DriverFirstName) || string.IsNullOrWhiteSpace(session.DriverLastName))
            {
                return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Driver information required",
                    "Driver first name and last name are required when drop-and-hook is disabled");
            }

            // 4. Build Toyota API payload
            var toyotaPayload = await BuildToyotaPayloadAsync(session, orders, exceptions);

            // GAP-003 & GAP-004: Validate Code 99 (Unplanned Expedite) rules
            var trailerExceptions = exceptions.Where(e => string.IsNullOrEmpty(e.RelatedSkidId)).ToList();
            var hasCode99 = trailerExceptions.Any(e => e.ExceptionType == "99");

            if (hasCode99)
            {
                // GAP-003: Route must start with "EX-" prefix when code 99 is used
                if (!session.RouteNumber.StartsWith("EX-", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                        "Code 99 validation failed",
                        "Unplanned Expedite (code 99) requires route to start with 'EX-' prefix");
                }

                // GAP-004: All skids must have at least one exception when code 99 is used
                var allSkidIds = new HashSet<string>();
                foreach (var order in orders)
                {
                    var skidScans = await _repository.GetSkidScansByOrderIdAsync(order.OrderId);
                    foreach (var scan in skidScans)
                    {
                        if (!string.IsNullOrEmpty(scan.RawSkidId))
                            allSkidIds.Add(scan.RawSkidId);
                    }
                }

                var skidExceptions = exceptions.Where(e => !string.IsNullOrEmpty(e.RelatedSkidId)).ToList();
                var skidsWithExceptions = new HashSet<string>(skidExceptions.Select(e => e.RelatedSkidId!));
                var skidsWithoutExceptions = allSkidIds.Except(skidsWithExceptions).ToList();

                if (skidsWithoutExceptions.Any())
                {
                    return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                        "Code 99 validation failed",
                        $"Unplanned Expedite (code 99) requires all skids to have exceptions. Missing exceptions for: {string.Join(", ", skidsWithoutExceptions)}");
                }
            }

            // 5. Submit to Toyota API (using default environment for now - could be configurable)
            var toyotaResponse = await _toyotaApiService.SubmitShipmentLoadAsync("QA", toyotaPayload);

            // 6. Update session with Toyota response
            session.ToyotaStatus = toyotaResponse.Success ? "confirmed" : "error";
            session.ToyotaConfirmationNumber = toyotaResponse.ConfirmationNumber;
            session.ToyotaErrorMessage = toyotaResponse.ErrorMessage;
            session.ToyotaSubmittedAt = DateTime.UtcNow;
            session.Status = toyotaResponse.Success ? "completed" : "error";
            session.CompletedAt = toyotaResponse.Success ? DateTime.UtcNow : null;
            session.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateSessionAsync(session);

            if (!toyotaResponse.Success)
            {
                _logger.LogError("Toyota API submission failed: {Error}", toyotaResponse.ErrorMessage);

                // Update all orders with error status
                foreach (var order in orders)
                {
                    order.Status = OrderStatus.ShipmentError;
                    order.ToyotaShipmentStatus = "error";
                    order.ToyotaShipmentErrorMessage = toyotaResponse.ErrorMessage ?? "Unknown error from Toyota API";
                    order.ToyotaShipmentSubmittedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;
                }
                await _repository.UpdateOrdersAsync(orders);

                return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Toyota API submission failed",
                    toyotaResponse.ErrorMessage ?? "Unknown error");
            }

            // 7. Update all orders with shipment details
            var completionTime = DateTime.UtcNow;
            var shippedOrderNumbers = new List<string>();

            foreach (var order in orders)
            {
                order.Status = OrderStatus.Shipped;
                order.ActualRoute = session.RouteNumber;
                order.ActualPickupDate = session.PickupDateTime ?? completionTime;
                order.Trailer = session.TrailerNumber;
                order.SealNumber = session.SealNumber;
                order.DriverName = $"{session.DriverFirstName} {session.DriverLastName}".Trim();
                order.CarrierName = session.LpCode;
                order.ShipmentConfirmation = toyotaResponse.ConfirmationNumber;
                order.ToyotaShipmentConfirmationNumber = toyotaResponse.ConfirmationNumber;
                order.ToyotaShipmentStatus = "confirmed";
                order.ToyotaShipmentSubmittedAt = completionTime;
                order.ShipmentLoadedAt = completionTime;
                order.UpdatedAt = completionTime;

                shippedOrderNumbers.Add(order.RealOrderNumber);
            }

            await _repository.UpdateOrdersAsync(orders);

            var response = new ShipmentLoadCompleteResponseDto
            {
                ConfirmationNumber = toyotaResponse.ConfirmationNumber ?? "N/A",
                RouteNumber = session.RouteNumber,
                TrailerNumber = session.TrailerNumber ?? "N/A",
                TotalOrdersShipped = orders.Count,
                CompletedAt = completionTime,
                ShippedOrderNumbers = shippedOrderNumbers
            };

            _logger.LogInformation(
                "Shipment completed: Route {RouteNumber}, Trailer {TrailerNumber}, Toyota Confirmation {ConfirmationNumber}, Orders: {OrderCount}",
                session.RouteNumber, session.TrailerNumber, toyotaResponse.ConfirmationNumber, orders.Count);

            return ApiResponse<ShipmentLoadCompleteResponseDto>.SuccessResponse(
                response,
                $"Shipment completed successfully. {orders.Count} orders shipped. Toyota Confirmation: {toyotaResponse.ConfirmationNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing shipment for session: {SessionId}", request.SessionId);
            return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                "Failed to complete shipment",
                ex.Message);
        }
    }

    // ===== VALIDATION OPERATIONS (without session) =====

    /// <summary>
    /// Validate order and get skid count WITHOUT starting a session
    /// </summary>
    public async Task<ApiResponse<ValidateOrderResponseDto>> ValidateOrderAsync(string orderNumber, string dockCode)
    {
        try
        {
            _logger.LogInformation("Validating order: {OrderNumber}-{DockCode}", orderNumber, dockCode);

            // 1. Find order by OrderNumber + DockCode
            var order = await _repository.GetOrderByNumberAndDockAsync(orderNumber, dockCode);

            if (order == null)
            {
                return ApiResponse<ValidateOrderResponseDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with OrderNumber '{orderNumber}' and DockCode '{dockCode}'");
            }

            // 2. Check if Status >= SkidBuilt
            bool skidBuildComplete = order.Status >= OrderStatus.SkidBuilt;

            // 3. Count skids from tblSkidScans for this order
            int skidCount = await _repository.GetSkidScansCountForOrderAsync(order.OrderId);

            // 4. Build response
            var response = new ValidateOrderResponseDto
            {
                Success = true,
                OrderId = order.OrderId,
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                PlantCode = order.PlantCode ?? string.Empty,
                SupplierCode = order.SupplierCode ?? string.Empty,
                Status = order.Status.ToString(),
                SkidBuildComplete = skidBuildComplete,
                SkidCount = skidCount,
                ToyotaConfirmationNumber = order.ToyotaSkidBuildConfirmationNumber,
                ToyotaShipmentConfirmationNumber = order.ToyotaShipmentConfirmationNumber
            };

            _logger.LogInformation(
                "Order validated: {OrderNumber}-{DockCode}, Status: {Status}, SkidBuildComplete: {SkidBuildComplete}, SkidCount: {SkidCount}",
                orderNumber, dockCode, order.Status, skidBuildComplete, skidCount);

            return ApiResponse<ValidateOrderResponseDto>.SuccessResponse(
                response,
                $"Order {orderNumber} validated successfully. Skid-build complete: {skidBuildComplete}, Skid count: {skidCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            return ApiResponse<ValidateOrderResponseDto>.ErrorResponse(
                "Failed to validate order",
                ex.Message);
        }
    }

    // ===== LEGACY OPERATIONS (backwards compatibility) =====

    /// <summary>
    /// Get all orders for a route that are ready to ship (legacy endpoint)
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadRouteResponseDto>> GetOrdersByRouteAsync(string routeNumber)
    {
        try
        {
            var orders = await _repository.GetOrdersByRouteAsync(routeNumber);

            if (orders == null || !orders.Any())
            {
                return ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                    "No orders found",
                    $"No orders ready to ship found for route {routeNumber}");
            }

            // Map to DTOs
            var orderDtos = new List<ShipmentLoadOrderDto>();
            foreach (var order in orders)
            {
                // Count skids for this order
                var skidCount = await _repository.GetSkidScansCountForOrderAsync(order.OrderId);

                orderDtos.Add(new ShipmentLoadOrderDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.RealOrderNumber,
                    DockCode = order.DockCode,
                    SupplierCode = order.SupplierCode,
                    PlantCode = order.PlantCode,
                    PlannedRoute = order.PlannedRoute,
                    Status = order.Status.ToString(),
                    TotalSkids = skidCount,
                    IsScanned = order.Status == OrderStatus.ShipmentLoading
                });
            }

            var response = new ShipmentLoadRouteResponseDto
            {
                RouteNumber = routeNumber,
                Orders = orderDtos,
                TotalOrders = orderDtos.Count
            };

            return ApiResponse<ShipmentLoadRouteResponseDto>.SuccessResponse(
                response,
                $"Found {orderDtos.Count} orders for route {routeNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for route: {RouteNumber}", routeNumber);
            return ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                "Failed to retrieve orders",
                ex.Message);
        }
    }

    // ===== HELPER METHODS =====

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
    /// Map ShipmentLoadSession entity to SessionResponseDto
    /// </summary>
    private async Task<SessionResponseDto> MapSessionToDtoAsync(
        ShipmentLoadSession session,
        List<Order> orders,
        List<ShipmentLoadException> exceptions,
        bool isResumed)
    {
        var (route, _) = ParseRouteAndRun(session.RouteNumber);

        // Get planned skids for all orders
        var plannedSkids = await GetPlannedSkidsAsync(orders, session.SessionId);

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

        var exceptionDtos = exceptions.Select(e => new ExceptionDto
        {
            ExceptionId = e.ExceptionId,
            ExceptionType = e.ExceptionType!,
            Comments = e.Comments,
            RelatedSkidId = e.RelatedSkidId,
            CreatedAt = e.CreatedAt
        }).ToList();

        return new SessionResponseDto
        {
            SessionId = session.SessionId,
            RouteNumber = session.RouteNumber,
            Route = route,
            Run = session.Run,
            SupplierCode = session.SupplierCode,
            PickupDateTime = session.PickupDateTime,
            Status = session.Status,
            TrailerNumber = session.TrailerNumber,
            SealNumber = session.SealNumber,
            LpCode = session.LpCode,
            DriverFirstName = session.DriverFirstName,
            DriverLastName = session.DriverLastName,
            SupplierFirstName = session.SupplierFirstName,
            SupplierLastName = session.SupplierLastName,
            Orders = orderDtos,
            Exceptions = exceptionDtos,
            PlannedSkids = plannedSkids,
            IsResumed = isResumed,
            CreatedAt = session.CreatedAt
        };
    }

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
    /// Build Toyota API payload from session, orders, and exceptions
    /// IMPORTANT: This method now includes BOTH shipment load exceptions AND skid build exceptions
    /// Maps Skid Build exception codes to Shipment Load codes and moves shortage to trailer level
    /// </summary>
    private async Task<ToyotaShipmentLoadRequest> BuildToyotaPayloadAsync(
        ShipmentLoadSession session,
        List<Order> orders,
        List<ShipmentLoadException> shipmentLoadExceptions)
    {
        // Parse route and run from session RouteNumber (e.g., "JAAJ-17" -> Route: "JAAJ", Run: "17")
        var (route, run) = ParseRouteAndRun(session.RouteNumber);

        var rootSupplierCode = orders.First().SupplierCode!;

        // GAP-010 to GAP-014: Validate field formats before building payload
        var routeValidation = _toyotaValidationService.ValidateRoute(route);
        if (!routeValidation.IsValid)
            throw new InvalidOperationException($"Route validation failed: {routeValidation.ErrorMessage}");

        var trailerValidation = _toyotaValidationService.ValidateTrailerNumber(session.TrailerNumber!);
        if (!trailerValidation.IsValid)
            throw new InvalidOperationException($"Trailer number validation failed: {trailerValidation.ErrorMessage}");

        if (!string.IsNullOrWhiteSpace(session.SealNumber))
        {
            var sealValidation = _toyotaValidationService.ValidateSealNumber(session.SealNumber);
            if (!sealValidation.IsValid)
                throw new InvalidOperationException($"Seal number validation failed: {sealValidation.ErrorMessage}");
        }

        if (!string.IsNullOrWhiteSpace(session.LpCode) && session.LpCode != "XXXX")
        {
            var lpCodeValidation = _toyotaValidationService.ValidateLpCode(session.LpCode);
            if (!lpCodeValidation.IsValid)
                throw new InvalidOperationException($"lpCode validation failed: {lpCodeValidation.ErrorMessage}");
        }

        var driverNameValidation = _toyotaValidationService.ValidateDriverName(
            session.DriverFirstName ?? "", session.DriverLastName ?? "");
        if (!driverNameValidation.IsValid)
            throw new InvalidOperationException($"Driver name validation failed: {driverNameValidation.ErrorMessage}");

        if (!string.IsNullOrWhiteSpace(session.SupplierFirstName) || !string.IsNullOrWhiteSpace(session.SupplierLastName))
        {
            var supplierNameValidation = _toyotaValidationService.ValidateSupplierName(
                session.SupplierFirstName ?? "", session.SupplierLastName ?? "");
            if (!supplierNameValidation.IsValid)
                throw new InvalidOperationException($"Supplier name validation failed: {supplierNameValidation.ErrorMessage}");
        }

        _logger.LogInformation(
            "Building Toyota payload - ROOT Level Supplier: {RootSupplier}, Route: {Route}, Run: {Run}, Trailer: {Trailer}",
            rootSupplierCode, route, run, session.TrailerNumber);

        // Collect all shortage exceptions from skid build to move to trailer level
        var allSkidBuildExceptions = new List<SkidBuildException>();
        foreach (var order in orders)
        {
            var orderExceptions = await _repository.GetSkidBuildExceptionsByOrderIdAsync(order.OrderId);
            allSkidBuildExceptions.AddRange(orderExceptions);
        }

        // Map Skid Build code 12 (Supplier Revised Shortage) to Shipment Load code 24 and add to trailer level
        var trailerExceptions = shipmentLoadExceptions
            .Where(e => string.IsNullOrEmpty(e.RelatedSkidId)) // Existing trailer-level exceptions
            .Select(e => new ToyotaException
            {
                ExceptionCode = e.ExceptionType!,
                Comments = e.Comments
            }).ToList();

        // Add shortage exceptions (code 12 from skid build) as code 24 at trailer level
        var shortageExceptions = allSkidBuildExceptions
            .Where(e => e.ExceptionCode == "12") // Skid Build shortage code
            .GroupBy(e => e.Comments ?? "") // Group by comments to avoid duplicates
            .Select(g => g.First()) // Take first from each group
            .Select(e => new ToyotaException
            {
                ExceptionCode = "24", // Shipment Load shortage code
                Comments = e.Comments
            }).ToList();

        trailerExceptions.AddRange(shortageExceptions);

        if (shortageExceptions.Any())
        {
            _logger.LogInformation(
                "Mapped {Count} Skid Build shortage exceptions (code 12) to Shipment Load code 24 at trailer level",
                shortageExceptions.Count);
        }

        var payload = new ToyotaShipmentLoadRequest
        {
            Supplier = rootSupplierCode, // ROOT LEVEL SUPPLIER (from first order)
            Route = route,
            Run = run, // Use run from session (e.g., "17" from JAAJ-17)
            TrailerNumber = session.TrailerNumber!,
            DropHook = false, // HARDCODED - VUTEQ business rule (driver always present)
            SealNumber = session.SealNumber,
            LpCode = string.IsNullOrEmpty(session.LpCode) ? "XXXX" : session.LpCode, // Default to XXXX if not provided
            DriverTeamFirstName = session.DriverFirstName,
            DriverTeamLastName = session.DriverLastName,
            SupplierTeamFirstName = session.SupplierFirstName,
            SupplierTeamLastName = session.SupplierLastName,
            Exceptions = trailerExceptions, // Now includes mapped shortage exceptions
            Orders = new List<ToyotaShipmentOrder>()
        };

        // Build orders with skids
        foreach (var order in orders)
        {
            var skidScans = await _repository.GetSkidScansByOrderIdAsync(order.OrderId);

            // Get skid build exceptions for this order
            var skidBuildExceptions = await _repository.GetSkidBuildExceptionsByOrderIdAsync(order.OrderId);

            var orderSupplierCode = order.SupplierCode!;

            // Group skids by unique (PalletizationCode + RawSkidId) to avoid duplicates
            // Toyota API SKID keyObject: Order + Supplier + Plant + Dock + Palletization + Skid
            // Multiple kanbans per manifest should result in ONE skid entry, not multiple
            var uniqueSkids = skidScans
                .GroupBy(s => new { s.PalletizationCode, s.RawSkidId })
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation(
                "Building Toyota payload - ORDER Level: Order={Order}, Supplier={OrderSupplier}, Plant={Plant}, Dock={Dock}, Skids: {TotalScans} kanban scans -> {UniqueSkids} unique skids, SkidBuildExceptions={ExceptionCount}",
                order.RealOrderNumber, orderSupplierCode, order.PlantCode, order.DockCode, skidScans.Count, uniqueSkids.Count, skidBuildExceptions.Count);

            var toyotaOrder = new ToyotaShipmentOrder
            {
                Order = order.RealOrderNumber,
                Supplier = orderSupplierCode, // ORDER LEVEL SUPPLIER (should match root level)
                Plant = order.PlantCode!,
                Dock = order.DockCode,
                PickUp = session.PickupDateTime?.ToString("yyyy-MM-ddTHH:mm") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"),
                Skids = uniqueSkids.Select(scan => new ToyotaShipmentSkid
                {
                    SkidId = scan.RawSkidId!, // Use RawSkidId which includes side (e.g., "001A")
                    Palletization = scan.PalletizationCode!,
                    SkidCut = scan.IsSkidCut,
                    Exceptions = BuildSkidExceptions(scan, shipmentLoadExceptions, skidBuildExceptions)
                }).ToList()
            };

            payload.Orders.Add(toyotaOrder);
        }

        _logger.LogInformation(
            "Toyota payload built - Total Orders: {OrderCount}, ROOT Supplier: {RootSupplier}, Trailer Exceptions (including mapped shortages): {TrailerExceptionCount}",
            payload.Orders.Count, payload.Supplier, payload.Exceptions.Count);

        return payload;
    }

    /// <summary>
    /// Build exceptions list for a specific skid
    /// Combines both shipment load exceptions and skid build exceptions
    /// Filters out shortage exceptions (code 12/24) - these go to trailer level only
    /// Only includes valid Shipment Load skid-level codes: 14, 15, 18, 19, 21, 22, 23
    /// </summary>
    /// <param name="scan">The skid scan record</param>
    /// <param name="shipmentLoadExceptions">Exceptions from shipment load session</param>
    /// <param name="skidBuildExceptions">Exceptions from skid build session</param>
    /// <returns>List of Toyota exceptions for this skid</returns>
    private List<ToyotaException> BuildSkidExceptions(
        SkidScan scan,
        List<ShipmentLoadException> shipmentLoadExceptions,
        List<SkidBuildException> skidBuildExceptions)
    {
        var exceptions = new List<ToyotaException>();

        // Valid skid-level exception codes for Shipment Load
        var validSkidLevelCodes = new HashSet<string> { "14", "15", "18", "19", "21", "22", "23" };

        // Add shipment load exceptions that match this skid's RawSkidId
        // Filter out code 24 (shortage) - it belongs at trailer level only
        var shipmentExceptions = shipmentLoadExceptions
            .Where(e => e.RelatedSkidId == scan.RawSkidId &&
                       validSkidLevelCodes.Contains(e.ExceptionType ?? ""))
            .Select(e => new ToyotaException
            {
                ExceptionCode = e.ExceptionType!,
                Comments = e.Comments
            });
        exceptions.AddRange(shipmentExceptions);

        // Add skid build exceptions for this skid
        // Match by SkidNumber (the numeric part without side, e.g., "001" matches "001A")
        // OR if exception has no SkidNumber (order-level exception), include it for all skids
        // Filter out code 12 (shortage) - it gets mapped to code 24 and moved to trailer level
        // Map other codes if needed (currently no other mappings needed)
        var skidExceptions = skidBuildExceptions
            .Where(e => (e.SkidNumber == null || // Order-level exception - include for all skids
                        e.SkidNumber.Value.ToString().PadLeft(3, '0') == scan.SkidNumber.ToString().PadLeft(3, '0')) && // Skid-level exception matching this skid
                        e.ExceptionCode != "12") // Filter out shortage - goes to trailer level as code 24
            .Select(e => new ToyotaException
            {
                ExceptionCode = MapSkidBuildToShipmentLoadCode(e.ExceptionCode),
                Comments = e.Comments
            });
        exceptions.AddRange(skidExceptions);

        if (exceptions.Any())
        {
            _logger.LogInformation(
                "Skid {SkidId} has {ExceptionCount} valid skid-level exceptions (ShipmentLoad: {ShipmentCount}, SkidBuild: {SkidBuildCount})",
                scan.RawSkidId,
                exceptions.Count,
                shipmentExceptions.Count(),
                skidExceptions.Count());
        }

        return exceptions;
    }

    /// <summary>
    /// Map Skid Build exception codes to Shipment Load exception codes
    /// Skid Build uses different codes than Shipment Load for the same exception
    /// </summary>
    /// <param name="skidBuildCode">Exception code from Skid Build (e.g., "10", "11", "12", "20")</param>
    /// <returns>Mapped Shipment Load exception code</returns>
    private string MapSkidBuildToShipmentLoadCode(string skidBuildCode)
    {
        // NOTE: Code 12 (shortage) is handled separately - it goes to trailer level as code 24
        // This method handles other codes that appear at skid level

        // Currently, other skid build codes (10, 11, 20) don't have skid-level equivalents in shipment load
        // They would typically be order-level or trailer-level exceptions
        // For now, return as-is, but this can be extended if needed

        return skidBuildCode switch
        {
            // Code 12 should never reach here - it's filtered out in BuildSkidExceptions
            "12" => throw new InvalidOperationException("Code 12 (shortage) should not be at skid level"),

            // Other codes - currently no mapping needed
            // If Toyota requires different codes for shipment load, add mappings here
            _ => skidBuildCode
        };
    }
}
