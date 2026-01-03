// Author: Hassan
// Date: 2025-12-01
// Description: Service for PlannedItem operations - handles business logic for planned items

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for PlannedItem service operations
/// </summary>
public interface IPlannedItemService
{
    Task<ApiResponse<IEnumerable<PlannedItemWithOrderDto>>> GetPlannedItemsAsync(Guid? uploadId = null, Guid? orderId = null);
}

/// <summary>
/// Service implementation for PlannedItem operations
/// </summary>
public class PlannedItemService : IPlannedItemService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PlannedItemService> _logger;

    public PlannedItemService(
        IOrderRepository orderRepository,
        ILogger<PlannedItemService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get planned items with order information, optionally filtered by upload ID or order ID
    /// </summary>
    public async Task<ApiResponse<IEnumerable<PlannedItemWithOrderDto>>> GetPlannedItemsAsync(Guid? uploadId = null, Guid? orderId = null)
    {
        try
        {
            // Get planned items based on filter (orderId takes precedence over uploadId)
            var plannedItems = orderId.HasValue
                ? await _orderRepository.GetPlannedItemsByOrderIdAsync(orderId.Value)
                : uploadId.HasValue
                    ? await _orderRepository.GetPlannedItemsByUploadIdAsync(uploadId.Value)
                    : await _orderRepository.GetAllPlannedItemsWithOrdersAsync();

            // Map to DTOs
            var plannedItemDtos = plannedItems.Select(pi => new PlannedItemWithOrderDto
            {
                PlannedItemId = pi.PlannedItemId,
                OrderId = pi.OrderId,
                RealOrderNumber = pi.Order.RealOrderNumber,
                DockCode = pi.Order.DockCode,
                PartNumber = pi.PartNumber,
                Qpc = pi.Qpc,
                KanbanNumber = pi.KanbanNumber,
                TotalBoxPlanned = pi.TotalBoxPlanned,
                ManifestNo = pi.ManifestNo,
                PalletizationCode = pi.PalletizationCode,
                ExternalOrderId = pi.ExternalOrderId,
                ShortOver = pi.ShortOver,
                CreatedAt = pi.CreatedAt,
                TotalScanned = pi.SkidScans?.Count ?? 0,
                RemainingBoxes = (pi.TotalBoxPlanned ?? 0) - (pi.SkidScans?.Count ?? 0),
                InternalKanban = pi.SkidScans != null && pi.SkidScans.Any(s => !string.IsNullOrEmpty(s.InternalKanban))
                    ? string.Join(", ", pi.SkidScans
                        .Where(s => !string.IsNullOrEmpty(s.InternalKanban))
                        .Select(s => s.InternalKanban)
                        .Distinct())
                    : null
            }).ToList();

            var message = orderId.HasValue
                ? $"Retrieved {plannedItemDtos.Count} planned item(s) for order {orderId.Value}"
                : uploadId.HasValue
                    ? $"Retrieved {plannedItemDtos.Count} planned item(s) for upload {uploadId.Value}"
                    : $"Retrieved {plannedItemDtos.Count} planned item(s)";

            _logger.LogInformation(message);

            return ApiResponse<IEnumerable<PlannedItemWithOrderDto>>.SuccessResponse(
                plannedItemDtos,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving planned items. UploadId: {UploadId}, OrderId: {OrderId}", uploadId, orderId);
            return ApiResponse<IEnumerable<PlannedItemWithOrderDto>>.ErrorResponse(
                "Failed to retrieve planned items",
                ex.Message);
        }
    }
}
