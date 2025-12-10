// Author: Hassan
// Date: 2025-12-08
// Description: Service for Shipment Load operations - handles business logic

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Shipment Load service operations
/// </summary>
public interface IShipmentLoadService
{
    Task<ApiResponse<ShipmentLoadRouteResponseDto>> GetOrdersByRouteAsync(string routeNumber);
    Task<ApiResponse<ShipmentLoadScanResponseDto>> ValidateAndScanSkidAsync(ShipmentLoadScanRequestDto request);
    Task<ApiResponse<ShipmentLoadCompleteResponseDto>> CompleteShipmentAsync(ShipmentLoadCompleteRequestDto request);
}

/// <summary>
/// Service implementation for Shipment Load operations
/// </summary>
public class ShipmentLoadService : IShipmentLoadService
{
    private readonly IShipmentLoadRepository _shipmentLoadRepository;
    private readonly ILogger<ShipmentLoadService> _logger;

    // System user ID for operations when user is not authenticated
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ShipmentLoadService(
        IShipmentLoadRepository shipmentLoadRepository,
        ILogger<ShipmentLoadService> logger)
    {
        _shipmentLoadRepository = shipmentLoadRepository;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the user ID from the request or uses system default
    /// Parses string userId to Guid, returns system user if null/empty/invalid
    /// </summary>
    private Guid ResolveUserId(string? userId)
    {
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedGuid))
        {
            return parsedGuid;
        }
        return SystemUserId;
    }

    /// <summary>
    /// Get all orders for a route that are ready to ship
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadRouteResponseDto>> GetOrdersByRouteAsync(string routeNumber)
    {
        try
        {
            var orders = await _shipmentLoadRepository.GetOrdersByRouteAsync(routeNumber);

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
                var skidCount = await _shipmentLoadRepository.GetSkidScansCountForOrderAsync(order.OrderId);

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

    /// <summary>
    /// Validate and scan a skid/order for shipment loading
    /// Validation rules:
    /// 1. Order exists with matching OrderNumber + DockCode
    /// 2. Order.Status >= SkidBuilt (was built)
    /// 3. Order.PlannedRoute matches current route
    /// 4. Order.Status != Shipped (not already shipped)
    /// 5. SkidScans exist for order (proves skid was built)
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadScanResponseDto>> ValidateAndScanSkidAsync(ShipmentLoadScanRequestDto request)
    {
        try
        {
            // 1. Check if order exists
            var order = await _shipmentLoadRepository.GetOrderByNumberAndDockAsync(
                request.OrderNumber, request.DockCode);

            if (order == null)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with number {request.OrderNumber} and dock code {request.DockCode}");
            }

            // 2. Check if order status is >= SkidBuilt
            if (order.Status < OrderStatus.SkidBuilt)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Order not ready",
                    $"Order {request.OrderNumber} has not been built yet (Status: {order.Status})");
            }

            // 3. Check if planned route matches
            if (order.PlannedRoute != request.RouteNumber)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Route mismatch",
                    $"Order {request.OrderNumber} is planned for route {order.PlannedRoute}, not {request.RouteNumber}");
            }

            // 4. Check if order is already shipped
            if (order.Status == OrderStatus.Shipped)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Already shipped",
                    $"Order {request.OrderNumber} has already been shipped");
            }

            // 5. Verify skid scans exist (proves skid was built)
            var skidScansCount = await _shipmentLoadRepository.GetSkidScansCountForOrderAsync(order.OrderId);
            if (skidScansCount == 0)
            {
                return ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "No skid scans found",
                    $"Order {request.OrderNumber} has no skid build scans. Please verify skid was built.");
            }

            // All validations passed - update order status to ShipmentLoading
            order.Status = OrderStatus.ShipmentLoading;
            order.UpdatedAt = DateTime.UtcNow;

            await _shipmentLoadRepository.UpdateOrdersAsync(new List<Models.Entities.Order> { order });

            var response = new ShipmentLoadScanResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                Status = order.Status.ToString(),
                ValidationMessage = $"Order {request.OrderNumber} validated successfully. {skidScansCount} skid(s) confirmed.",
                ScannedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Order scanned for shipment: {OrderNumber}-{DockCode} on route {RouteNumber}",
                request.OrderNumber, request.DockCode, request.RouteNumber);

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

    /// <summary>
    /// Complete shipment for a route - updates all scanned orders to Shipped status
    /// </summary>
    public async Task<ApiResponse<ShipmentLoadCompleteResponseDto>> CompleteShipmentAsync(ShipmentLoadCompleteRequestDto request)
    {
        try
        {
            // Get all orders in ShipmentLoading status for this route
            var ordersToShip = await _shipmentLoadRepository.GetOrdersByRouteAndStatusAsync(
                request.RouteNumber, OrderStatus.ShipmentLoading);

            if (ordersToShip == null || !ordersToShip.Any())
            {
                return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "No orders to ship",
                    $"No orders in ShipmentLoading status found for route {request.RouteNumber}");
            }

            // Generate confirmation number: SL-{timestamp}-{random}
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var random = new Random().Next(1000, 9999);
            var confirmationNumber = $"SL-{timestamp}-{random}";

            // Resolve user ID
            var resolvedUserId = ResolveUserId(request.UserId);

            // Update all orders
            var shippedOrderNumbers = new List<string>();
            var completionTime = DateTime.UtcNow;

            foreach (var order in ordersToShip)
            {
                order.Status = OrderStatus.Shipped;
                order.ActualRoute = request.RouteNumber;
                order.ActualPickupDate = completionTime;
                order.Trailer = request.TrailerNumber;
                order.SealNumber = request.SealNumber;
                order.DriverName = request.DriverName;
                order.CarrierName = request.CarrierName;
                order.ShipmentNotes = request.ShipmentNotes;
                order.ShipmentConfirmation = confirmationNumber;
                order.ShipmentLoadedAt = completionTime;
                order.UpdatedAt = completionTime;

                shippedOrderNumbers.Add(order.RealOrderNumber);
            }

            // Bulk update all orders
            await _shipmentLoadRepository.UpdateOrdersAsync(ordersToShip);

            var response = new ShipmentLoadCompleteResponseDto
            {
                ConfirmationNumber = confirmationNumber,
                RouteNumber = request.RouteNumber,
                TrailerNumber = request.TrailerNumber,
                TotalOrdersShipped = ordersToShip.Count,
                CompletedAt = completionTime,
                ShippedOrderNumbers = shippedOrderNumbers
            };

            _logger.LogInformation(
                "Shipment completed: Route {RouteNumber}, Trailer {TrailerNumber}, Confirmation {ConfirmationNumber}, Orders: {OrderCount}",
                request.RouteNumber, request.TrailerNumber, confirmationNumber, ordersToShip.Count);

            return ApiResponse<ShipmentLoadCompleteResponseDto>.SuccessResponse(
                response,
                $"Shipment completed successfully. {ordersToShip.Count} orders shipped. Confirmation: {confirmationNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing shipment for route: {RouteNumber}", request.RouteNumber);
            return ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                "Failed to complete shipment",
                ex.Message);
        }
    }
}
