// Author: Hassan
// Date: 2025-12-17
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
    private readonly ILogger<ShipmentLoadService> _logger;

    // System user ID for operations when user is not authenticated
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ShipmentLoadService(
        IShipmentLoadRepository repository,
        IToyotaApiService toyotaApiService,
        ILogger<ShipmentLoadService> logger)
    {
        _repository = repository;
        _toyotaApiService = toyotaApiService;
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

            // Check if active session exists for this route
            var existingSession = await _repository.GetActiveSessionByRouteAsync(request.RouteNumber);

            bool isResumed = false;

            if (existingSession != null)
            {
                _logger.LogInformation("Resuming existing session: {SessionId}", existingSession.SessionId);
                isResumed = true;
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

            // Get orders linked to this session
            var orders = await _repository.GetOrdersBySessionIdAsync(existingSession.SessionId);

            // Get exceptions for this session
            var exceptions = await _repository.GetSessionExceptionsAsync(existingSession.SessionId);

            // Map to response DTO
            var response = MapSessionToDto(existingSession, orders, exceptions, isResumed);
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

            var orders = await _repository.GetOrdersBySessionIdAsync(sessionId);
            var exceptions = await _repository.GetSessionExceptionsAsync(sessionId);
            var response = MapSessionToDto(session, orders, exceptions, false);

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

            var orders = await _repository.GetOrdersBySessionIdAsync(sessionId);
            var exceptions = await _repository.GetSessionExceptionsAsync(sessionId);
            var response = MapSessionToDto(session, orders, exceptions, false);

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

            // 4. Build Toyota API payload
            var toyotaPayload = await BuildToyotaPayloadAsync(session, orders, exceptions);

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
                ToyotaConfirmationNumber = order.ToyotaSkidBuildConfirmationNumber
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
    /// </summary>
    private (string route, string run) ParseRouteAndRun(string routeNumber)
    {
        if (string.IsNullOrEmpty(routeNumber) || routeNumber.Length < 2)
            return (routeNumber, "");

        // Run is last 2 characters, Route is everything before
        var run = routeNumber.Substring(routeNumber.Length - 2);
        var route = routeNumber.Substring(0, routeNumber.Length - 2);

        return (route, run);
    }

    /// <summary>
    /// Map ShipmentLoadSession entity to SessionResponseDto
    /// </summary>
    private SessionResponseDto MapSessionToDto(
        ShipmentLoadSession session,
        List<Order> orders,
        List<ShipmentLoadException> exceptions,
        bool isResumed)
    {
        var (route, _) = ParseRouteAndRun(session.RouteNumber);

        var orderDtos = orders.Select(o => new ShipmentLoadOrderDto
        {
            OrderId = o.OrderId,
            OrderNumber = o.RealOrderNumber,
            DockCode = o.DockCode,
            SupplierCode = o.SupplierCode,
            PlantCode = o.PlantCode,
            PlannedRoute = o.PlannedRoute,
            Status = o.Status.ToString(),
            TotalSkids = 0, // Would need to query if needed
            IsScanned = o.Status == OrderStatus.ShipmentLoading || o.ShipmentLoadSessionId == session.SessionId
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
            IsResumed = isResumed,
            CreatedAt = session.CreatedAt
        };
    }

    /// <summary>
    /// Build Toyota API payload from session, orders, and exceptions
    /// </summary>
    private async Task<ToyotaShipmentLoadRequest> BuildToyotaPayloadAsync(
        ShipmentLoadSession session,
        List<Order> orders,
        List<ShipmentLoadException> exceptions)
    {
        var (route, run) = ParseRouteAndRun(session.RouteNumber);

        var rootSupplierCode = orders.First().SupplierCode!;

        _logger.LogInformation(
            "Building Toyota payload - ROOT Level Supplier: {RootSupplier}, Route: {Route}, Run: {Run}, Trailer: {Trailer}",
            rootSupplierCode, route, run, session.TrailerNumber);

        var payload = new ToyotaShipmentLoadRequest
        {
            Supplier = rootSupplierCode, // ROOT LEVEL SUPPLIER (from first order)
            Route = route,
            Run = session.Run ?? run,
            TrailerNumber = session.TrailerNumber!,
            DropHook = false, // HARDCODED - VUTEQ business rule (driver always present)
            SealNumber = session.SealNumber,
            LpCode = session.LpCode,
            DriverTeamFirstName = session.DriverFirstName,
            DriverTeamLastName = session.DriverLastName,
            SupplierTeamFirstName = session.SupplierFirstName,
            SupplierTeamLastName = session.SupplierLastName,
            Exceptions = exceptions
                .Where(e => string.IsNullOrEmpty(e.RelatedSkidId)) // Trailer-level exceptions only
                .Select(e => new ToyotaException
                {
                    ExceptionCode = e.ExceptionType!,
                    Comments = e.Comments
                }).ToList(),
            Orders = new List<ToyotaShipmentOrder>()
        };

        // Build orders with skids
        foreach (var order in orders)
        {
            var skidScans = await _repository.GetSkidScansByOrderIdAsync(order.OrderId);

            var orderSupplierCode = order.SupplierCode!;

            _logger.LogInformation(
                "Building Toyota payload - ORDER Level: Order={Order}, Supplier={OrderSupplier}, Plant={Plant}, Dock={Dock}, Skids={SkidCount}",
                order.RealOrderNumber, orderSupplierCode, order.PlantCode, order.DockCode, skidScans.Count);

            var toyotaOrder = new ToyotaShipmentOrder
            {
                Order = order.RealOrderNumber,
                Supplier = orderSupplierCode, // ORDER LEVEL SUPPLIER (should match root level)
                Plant = order.PlantCode!,
                Dock = order.DockCode,
                PickUp = session.PickupDateTime?.ToString("yyyy-MM-ddTHH:mm") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"),
                Skids = skidScans.Select(scan => new ToyotaShipmentSkid
                {
                    SkidId = scan.SkidNumber, // Toyota spec: numeric only (3 digits)
                    Palletization = scan.PalletizationCode!,
                    SkidCut = scan.IsSkidCut,
                    Exceptions = exceptions
                        .Where(e => e.RelatedSkidId == scan.SkidNumber) // Match by SkidNumber only (numeric)
                        .Select(e => new ToyotaException
                        {
                            ExceptionCode = e.ExceptionType!,
                            Comments = e.Comments
                        }).ToList()
                }).ToList()
            };

            payload.Orders.Add(toyotaOrder);
        }

        _logger.LogInformation(
            "Toyota payload built - Total Orders: {OrderCount}, ROOT Supplier: {RootSupplier}",
            payload.Orders.Count, payload.Supplier);

        return payload;
    }
}
