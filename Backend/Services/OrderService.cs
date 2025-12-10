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
                UploadId = o.UploadId
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
}
