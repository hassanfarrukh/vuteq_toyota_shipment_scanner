// Author: Hassan
// Date: 2025-12-24
// Description: Service for Dock Monitor - handles business logic for real-time dock monitoring

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;
using System.Text.Json;

namespace Backend.Services;

/// <summary>
/// Interface for Dock Monitor service operations
/// </summary>
public interface IDockMonitorService
{
    /// <summary>
    /// Get dock monitor data with all shipments and orders
    /// </summary>
    Task<ApiResponse<DockMonitorResponseDto>> GetDockMonitorDataAsync();
}

/// <summary>
/// Service implementation for Dock Monitor
/// </summary>
public class DockMonitorService : IDockMonitorService
{
    private readonly IDockMonitorRepository _dockMonitorRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<DockMonitorService> _logger;

    public DockMonitorService(
        IDockMonitorRepository dockMonitorRepository,
        ISettingsRepository settingsRepository,
        ILogger<DockMonitorService> logger)
    {
        _dockMonitorRepository = dockMonitorRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get dock monitor data with all shipments and orders
    /// </summary>
    public async Task<ApiResponse<DockMonitorResponseDto>> GetDockMonitorDataAsync()
    {
        try
        {
            // Get dock monitor settings
            var settings = await _settingsRepository.GetDockMonitorSettingsAsync();
            var settingsDto = MapToDockMonitorSettingsDto(settings);

            // Get orders from last 36 hours (1.5 days)
            var orders = await _dockMonitorRepository.GetRecentOrdersWithShipmentsAsync(36);

            // Get shipment sessions
            var sessions = await _dockMonitorRepository.GetRecentShipmentSessionsAsync(36);

            // Get exception types for all orders
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var exceptions = await _dockMonitorRepository.GetOrderExceptionTypesAsync(orderIds);

            // Build shipments dictionary (group by ShipmentLoadSessionId or MainRoute)
            var shipments = BuildShipments(orders, sessions, exceptions, settingsDto);

            // Apply display mode filter
            var filteredShipments = ApplyDisplayModeFilter(shipments, settingsDto.DisplayMode);

            // Apply location filter
            var locationFilteredShipments = ApplyLocationFilter(filteredShipments, settingsDto.SelectedLocations);

            // Build response
            var response = new DockMonitorResponseDto
            {
                Shipments = locationFilteredShipments.OrderBy(s => s.PickupDateTime ?? DateTime.MaxValue).ToList(),
                TotalOrders = locationFilteredShipments.Sum(s => s.Orders.Count),
                Settings = settingsDto,
                RefreshedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Dock monitor data retrieved: {ShipmentCount} shipments, {OrderCount} orders",
                response.Shipments.Count,
                response.TotalOrders);

            return ApiResponse<DockMonitorResponseDto>.SuccessResponse(
                response,
                "Dock monitor data retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dock monitor data");
            return ApiResponse<DockMonitorResponseDto>.ErrorResponse(
                "Failed to retrieve dock monitor data",
                ex.Message);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Build shipments grouped by ShipmentLoadSessionId or MainRoute
    /// </summary>
    private List<DockMonitorShipmentDto> BuildShipments(
        List<Order> orders,
        List<ShipmentLoadSession> sessions,
        Dictionary<Guid, List<string>> exceptions,
        DockMonitorSettingsDto settings)
    {
        var shipmentDict = new Dictionary<string, DockMonitorShipmentDto>();
        var sessionDict = sessions.ToDictionary(s => s.SessionId, s => s);

        foreach (var order in orders)
        {
            // Determine grouping key (SessionId or MainRoute)
            string groupKey;
            ShipmentLoadSession? session = null;

            if (order.ShipmentLoadSessionId.HasValue &&
                sessionDict.TryGetValue(order.ShipmentLoadSessionId.Value, out session))
            {
                groupKey = session.SessionId.ToString();
            }
            else if (!string.IsNullOrEmpty(order.MainRoute))
            {
                groupKey = order.MainRoute;
            }
            else
            {
                // No grouping - create individual shipment
                groupKey = $"ORDER_{order.OrderId}";
            }

            // Get or create shipment
            if (!shipmentDict.ContainsKey(groupKey))
            {
                shipmentDict[groupKey] = new DockMonitorShipmentDto
                {
                    RouteNumber = session?.RouteNumber ?? order.MainRoute ?? order.RealOrderNumber,
                    Run = session?.Run ?? ExtractRun(order.MainRoute),
                    SupplierCode = session?.SupplierCode ?? order.SupplierCode,
                    PickupDateTime = session?.PickupDateTime ?? order.PlannedPickup,
                    ShipmentStatus = session?.Status ?? "pending",
                    CompletedAt = session?.CompletedAt,
                    Orders = new List<DockMonitorOrderDto>()
                };
            }

            // Add order to shipment
            var orderDto = BuildOrderDto(order, exceptions, settings);
            shipmentDict[groupKey].Orders.Add(orderDto);
        }

        return shipmentDict.Values.ToList();
    }

    /// <summary>
    /// Build individual order DTO with status calculation
    /// </summary>
    private DockMonitorOrderDto BuildOrderDto(
        Order order,
        Dictionary<Guid, List<string>> exceptions,
        DockMonitorSettingsDto settings)
    {
        var orderDto = new DockMonitorOrderDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.RealOrderNumber,
            DockCode = order.DockCode,
            Destination = GetDestination(order),
            SupplierCode = order.SupplierCode,
            PlannedPickup = order.PlannedPickup,
            PlannedSkidBuild = order.PlannedPickup?.AddHours(-2), // 2 hours before pickup
            CompletedSkidBuild = order.ToyotaSkidBuildSubmittedAt,
            PlannedShipmentLoad = order.PlannedPickup,
            CompletedShipmentLoad = order.ToyotaShipmentSubmittedAt,
            IsSupplementOrder = false, // TODO: Determine supplement order logic
            ToyotaSkidBuildStatus = order.ToyotaSkidBuildStatus,
            ToyotaShipmentStatus = order.ToyotaShipmentStatus
        };

        // Calculate status
        orderDto.Status = CalculateOrderStatus(order, exceptions, settings);

        return orderDto;
    }

    /// <summary>
    /// Calculate order status based on completion times and thresholds
    /// </summary>
    private string CalculateOrderStatus(
        Order order,
        Dictionary<Guid, List<string>> exceptions,
        DockMonitorSettingsDto settings)
    {
        // Check for exception-based statuses first
        // Exception codes: "10" (Revised Quantity), "11" (Modified QPC), "12" (Short Shipment), "20" (Non-Standard Packaging)
        if (exceptions.TryGetValue(order.OrderId, out var exceptionList))
        {
            // "12" = Short Shipment
            if (exceptionList.Any(e => e == "12"))
            {
                return "SHORT_SHIPPED";
            }
            // "10" or "11" could indicate projected short/revisions
            if (exceptionList.Any(e => e == "10" || e == "11"))
            {
                return "PROJECT_SHORT";
            }
        }

        // Check if both skid build and shipment load are completed
        if (order.ToyotaSkidBuildSubmittedAt.HasValue && order.ToyotaShipmentSubmittedAt.HasValue)
        {
            return "COMPLETED";
        }

        // Calculate time-based status
        if (order.PlannedPickup.HasValue)
        {
            var now = DateTime.UtcNow;
            var plannedSkidBuild = order.PlannedPickup.Value.AddHours(-2);
            var minutesLate = (int)(now - plannedSkidBuild).TotalMinutes;

            // If skid build is done but not shipment
            if (order.ToyotaSkidBuildSubmittedAt.HasValue && !order.ToyotaShipmentSubmittedAt.HasValue)
            {
                // Check against shipment load planned time
                minutesLate = (int)(now - order.PlannedPickup.Value).TotalMinutes;
            }

            if (minutesLate >= settings.CriticalThreshold)
            {
                return "CRITICAL";
            }
            if (minutesLate >= settings.BehindThreshold)
            {
                return "BEHIND";
            }
        }

        return "ON_TIME";
    }

    /// <summary>
    /// Apply display mode filter
    /// </summary>
    private List<DockMonitorShipmentDto> ApplyDisplayModeFilter(
        List<DockMonitorShipmentDto> shipments,
        string displayMode)
    {
        switch (displayMode?.ToUpper())
        {
            case "SHIPMENT_ONLY":
                // Show only orders with shipment load completed
                foreach (var shipment in shipments)
                {
                    shipment.Orders = shipment.Orders
                        .Where(o => o.CompletedShipmentLoad.HasValue)
                        .ToList();
                }
                return shipments.Where(s => s.Orders.Any()).ToList();

            case "SKID_ONLY":
                // Show only orders with skid build completed but shipment not yet done
                foreach (var shipment in shipments)
                {
                    shipment.Orders = shipment.Orders
                        .Where(o => o.CompletedSkidBuild.HasValue && !o.CompletedShipmentLoad.HasValue)
                        .ToList();
                }
                return shipments.Where(s => s.Orders.Any()).ToList();

            case "COMPLETION_ONLY":
                // Show only fully completed orders
                foreach (var shipment in shipments)
                {
                    shipment.Orders = shipment.Orders
                        .Where(o => o.Status == "COMPLETED")
                        .ToList();
                }
                return shipments.Where(s => s.Orders.Any()).ToList();

            case "FULL":
            default:
                // Show all orders
                return shipments;
        }
    }

    /// <summary>
    /// Apply location filter based on selected locations
    /// </summary>
    private List<DockMonitorShipmentDto> ApplyLocationFilter(
        List<DockMonitorShipmentDto> shipments,
        List<string> selectedLocations)
    {
        // If no locations selected, return all
        if (selectedLocations == null || !selectedLocations.Any())
        {
            return shipments;
        }

        // Filter based on plant/location
        // TODO: Implement location filtering logic based on PlantCode or other fields
        // For now, return all shipments
        return shipments;
    }

    /// <summary>
    /// Extract run code from route (last 2 characters)
    /// </summary>
    private string? ExtractRun(string? route)
    {
        if (string.IsNullOrEmpty(route) || route.Length < 2)
        {
            return null;
        }
        return route.Substring(route.Length - 2);
    }

    /// <summary>
    /// Get destination from dock code or plant
    /// </summary>
    private string? GetDestination(Order order)
    {
        // Map dock codes to destinations
        // TODO: Implement proper dock code to destination mapping
        // For now, return plant code or dock code
        return order.PlantCode ?? order.DockCode;
    }

    /// <summary>
    /// Map DockMonitorSetting entity to DTO
    /// </summary>
    private DockMonitorSettingsDto MapToDockMonitorSettingsDto(DockMonitorSetting? settings)
    {
        if (settings == null)
        {
            // Return default settings
            return new DockMonitorSettingsDto
            {
                SettingId = Guid.Empty,
                UserId = null,
                BehindThreshold = 15,
                CriticalThreshold = 30,
                DisplayMode = "FULL",
                SelectedLocations = new List<string>(),
                RefreshInterval = 300000,
                ModifiedAt = DateTime.UtcNow
            };
        }

        List<string> locations = new List<string>();
        if (!string.IsNullOrEmpty(settings.SelectedLocations))
        {
            try
            {
                locations = JsonSerializer.Deserialize<List<string>>(settings.SelectedLocations)
                    ?? new List<string>();
            }
            catch
            {
                locations = new List<string>();
            }
        }

        return new DockMonitorSettingsDto
        {
            SettingId = settings.SettingId,
            UserId = settings.UserId,
            BehindThreshold = settings.BehindThreshold,
            CriticalThreshold = settings.CriticalThreshold,
            DisplayMode = settings.DisplayMode,
            SelectedLocations = locations,
            RefreshInterval = settings.RefreshInterval,
            ModifiedAt = settings.UpdatedAt ?? settings.CreatedAt
        };
    }

    #endregion
}
