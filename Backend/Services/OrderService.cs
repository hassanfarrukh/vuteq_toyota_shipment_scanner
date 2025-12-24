// Author: Hassan
// Date: 2025-12-01
// Description: Service for Order operations - handles business logic for orders

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Order service operations
/// </summary>
public interface IOrderService
{
    Task<ApiResponse<IEnumerable<OrderListDto>>> GetOrdersAsync(Guid? uploadId = null);
    Task<ApiResponse<OrderSkidsResponseDto>> GetOrderSkidsAsync(string orderNumber, string dockCode);
}

/// <summary>
/// Service implementation for Order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get orders with TotalParts count, optionally filtered by upload ID
    /// </summary>
    public async Task<ApiResponse<IEnumerable<OrderListDto>>> GetOrdersAsync(Guid? uploadId = null)
    {
        try
        {
            // Get orders based on filter
            var orders = uploadId.HasValue
                ? await _orderRepository.GetOrdersByUploadIdAsync(uploadId.Value)
                : await _orderRepository.GetAllOrdersAsync();

            // Map to DTOs with TotalParts count
            var orderDtos = orders.Select(o => new OrderListDto
            {
                OrderId = o.OrderId,
                RealOrderNumber = o.RealOrderNumber,
                TotalParts = o.PlannedItems?.Count ?? 0,
                DockCode = o.DockCode,
                DepartureDate = o.PlannedPickup, // Fixed 2025-12-09 - Use PlannedPickup as DepartureDate
                OrderDate = o.TransmitDate,
                Status = o.Status.ToString(),
                UploadId = o.UploadId,
                PlannedRoute = o.PlannedRoute,
                MainRoute = o.MainRoute
            }).ToList();

            var message = uploadId.HasValue
                ? $"Retrieved {orderDtos.Count} order(s) for upload {uploadId.Value}"
                : $"Retrieved {orderDtos.Count} order(s)";

            _logger.LogInformation(message);

            return ApiResponse<IEnumerable<OrderListDto>>.SuccessResponse(
                orderDtos,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders. UploadId: {UploadId}", uploadId);
            return ApiResponse<IEnumerable<OrderListDto>>.ErrorResponse(
                "Failed to retrieve orders",
                ex.Message);
        }
    }

    /// <summary>
    /// Get distinct skids built for an order
    /// </summary>
    public async Task<ApiResponse<OrderSkidsResponseDto>> GetOrderSkidsAsync(string orderNumber, string dockCode)
    {
        try
        {
            // Get order with all skid scans
            var order = await _orderRepository.GetOrderWithSkidScansAsync(orderNumber, dockCode);

            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderNumber}-{DockCode}", orderNumber, dockCode);
                return ApiResponse<OrderSkidsResponseDto>.ErrorResponse(
                    $"Order {orderNumber} with dock code {dockCode} not found");
            }

            // Get all skid scans from all planned items
            var allSkidScans = order.PlannedItems
                .SelectMany(pi => pi.SkidScans)
                .ToList();

            // DEBUG: Log total skid scans found
            _logger.LogInformation("Total SkidScans found for order {OrderNumber}: {Count}", orderNumber, allSkidScans.Count);

            // DEBUG: Log each skid scan's grouping keys
            foreach (var scan in allSkidScans)
            {
                _logger.LogInformation("SkidScan - SkidNumber: '{SkidNumber}', PalletizationCode: '{PalletizationCode}', SkidSide: '{SkidSide}'",
                    scan.SkidNumber ?? "NULL", scan.PalletizationCode ?? "NULL", scan.SkidSide ?? "NULL");
            }

            // Group by SkidNumber + PalletizationCode (same as Toyota API payload structure)
            // Each unique combination of SkidNumber + PalletizationCode = one skid to load
            // FIXED: Filter out null values before grouping
            var distinctSkids = allSkidScans
                .Where(s => !string.IsNullOrEmpty(s.SkidNumber) && !string.IsNullOrEmpty(s.PalletizationCode))
                .GroupBy(s => new { s.SkidNumber, s.PalletizationCode })
                .Select(g => new SkidDto
                {
                    // Build SkidId from SkidNumber + SkidSide of first scan in group
                    SkidId = $"{g.Key.SkidNumber}{g.First().SkidSide}".TrimEnd(),
                    SkidNumber = g.Key.SkidNumber,
                    SkidSide = g.First().SkidSide,
                    PalletizationCode = g.Key.PalletizationCode,
                    ScannedAt = g.Min(s => s.ScannedAt)  // First scan time
                })
                .OrderBy(s => s.SkidNumber)
                .ThenBy(s => s.PalletizationCode)
                .ToList();

            // DEBUG: Log distinct skids after grouping
            _logger.LogInformation("Distinct skids after grouping: {Count}", distinctSkids.Count);
            foreach (var skid in distinctSkids)
            {
                _logger.LogInformation("Grouped Skid - SkidNumber: '{SkidNumber}', PalletizationCode: '{PalletizationCode}'",
                    skid.SkidNumber, skid.PalletizationCode);
            }

            var responseDto = new OrderSkidsResponseDto
            {
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                OrderId = order.OrderId,
                Skids = distinctSkids,
                TotalSkids = distinctSkids.Count
            };

            var message = distinctSkids.Count > 0
                ? $"Found {distinctSkids.Count} skid(s) for order {orderNumber}"
                : $"No skids found for order {orderNumber}";

            _logger.LogInformation("Retrieved {Count} skid(s) for order {OrderNumber}-{DockCode}",
                distinctSkids.Count, orderNumber, dockCode);

            return ApiResponse<OrderSkidsResponseDto>.SuccessResponse(
                responseDto,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skids for order: {OrderNumber}-{DockCode}",
                orderNumber, dockCode);
            return ApiResponse<OrderSkidsResponseDto>.ErrorResponse(
                "Failed to retrieve order skids",
                ex.Message);
        }
    }
}
