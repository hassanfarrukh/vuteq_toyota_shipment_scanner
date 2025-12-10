// Author: Hassan
// Date: 2025-12-08
// Description: Repository for Shipment Load operations - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Shipment Load repository operations
/// </summary>
public interface IShipmentLoadRepository
{
    // Order operations
    Task<List<Order>> GetOrdersByRouteAsync(string routeNumber);
    Task<Order?> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode);
    Task<int> GetSkidScansCountForOrderAsync(Guid orderId);
    Task UpdateOrdersAsync(List<Order> orders);
    Task<List<Order>> GetOrdersByRouteAndStatusAsync(string routeNumber, OrderStatus status);
}

/// <summary>
/// Repository implementation for Shipment Load operations
/// </summary>
public class ShipmentLoadRepository : IShipmentLoadRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<ShipmentLoadRepository> _logger;

    public ShipmentLoadRepository(VuteqDbContext context, ILogger<ShipmentLoadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders for a route where Status >= SkidBuilt (ready to ship)
    /// </summary>
    public async Task<List<Order>> GetOrdersByRouteAsync(string routeNumber)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .Where(o =>
                    o.PlannedRoute == routeNumber &&
                    o.Status >= OrderStatus.SkidBuilt)
                .OrderBy(o => o.RealOrderNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for route: {RouteNumber}", routeNumber);
            throw;
        }
    }

    /// <summary>
    /// Get orders by route and specific status
    /// </summary>
    public async Task<List<Order>> GetOrdersByRouteAndStatusAsync(string routeNumber, OrderStatus status)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .Where(o =>
                    o.PlannedRoute == routeNumber &&
                    o.Status == status)
                .OrderBy(o => o.RealOrderNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for route {RouteNumber} with status {Status}",
                routeNumber, status);
            throw;
        }
    }

    /// <summary>
    /// Get order by order number and dock code
    /// </summary>
    public async Task<Order?> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .FirstOrDefaultAsync(o =>
                    o.RealOrderNumber == orderNumber &&
                    o.DockCode == dockCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Get count of skid scans for an order to verify skid was built
    /// </summary>
    public async Task<int> GetSkidScansCountForOrderAsync(Guid orderId)
    {
        try
        {
            // Get all PlannedItemIds for this order
            var plannedItemIds = await _context.PlannedItems
                .Where(pi => pi.OrderId == orderId)
                .Select(pi => pi.PlannedItemId)
                .ToListAsync();

            if (!plannedItemIds.Any())
                return 0;

            // Count all skid scans for these planned items
            return await _context.SkidScans
                .Where(s => plannedItemIds.Contains(s.PlannedItemId))
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skid scans count for order: {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Bulk update orders (for shipment completion)
    /// </summary>
    public async Task UpdateOrdersAsync(List<Order> orders)
    {
        try
        {
            _context.Orders.UpdateRange(orders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated {Count} orders", orders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating orders");
            throw;
        }
    }
}
