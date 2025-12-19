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
    // Session operations
    Task<ShipmentLoadSession?> GetActiveSessionByRouteAsync(string routeNumber);
    Task<ShipmentLoadSession?> GetSessionByIdAsync(Guid sessionId);
    Task<ShipmentLoadSession> CreateSessionAsync(ShipmentLoadSession session);
    Task<ShipmentLoadSession> UpdateSessionAsync(ShipmentLoadSession session);
    Task<ShipmentLoadSession?> GetSessionWithOrdersAsync(Guid sessionId);
    Task<List<ShipmentLoadException>> GetSessionExceptionsAsync(Guid sessionId);

    // Order operations
    Task<List<Order>> GetOrdersByRouteAsync(string routeNumber);
    Task<Order?> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode);
    Task<int> GetSkidScansCountForOrderAsync(Guid orderId);
    Task UpdateOrdersAsync(List<Order> orders);
    Task<List<Order>> GetOrdersByRouteAndStatusAsync(string routeNumber, OrderStatus status);
    Task<List<Order>> GetOrdersBySessionIdAsync(Guid sessionId);
    Task LinkOrderToSessionAsync(Guid orderId, Guid sessionId);

    // Skid operations
    Task<List<SkidScan>> GetSkidScansByOrderIdAsync(Guid orderId);

    // Exception operations
    Task<ShipmentLoadException> AddExceptionAsync(ShipmentLoadException exception);
    Task DeleteExceptionAsync(Guid exceptionId);
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
    /// Normalizes route comparison by removing hyphens (e.g., JAAJ17 matches JAAJ-17)
    /// </summary>
    public async Task<List<Order>> GetOrdersByRouteAsync(string routeNumber)
    {
        try
        {
            // Normalize input route by removing hyphens
            var normalizedRoute = routeNumber.Replace("-", "");

            // Also try with hyphen inserted before last 2 digits (e.g., JAAJ17 -> JAAJ-17)
            var routeWithHyphen = normalizedRoute.Length > 2
                ? normalizedRoute.Insert(normalizedRoute.Length - 2, "-")
                : normalizedRoute;

            return await _context.Orders
                .Include(o => o.PlannedItems)
                .Where(o =>
                    (o.PlannedRoute == normalizedRoute ||
                     o.PlannedRoute == routeWithHyphen ||
                     o.PlannedRoute == routeNumber) &&
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
    /// Normalizes route comparison by removing hyphens (e.g., JAAJ17 matches JAAJ-17)
    /// </summary>
    public async Task<List<Order>> GetOrdersByRouteAndStatusAsync(string routeNumber, OrderStatus status)
    {
        try
        {
            // Normalize input route by removing hyphens
            var normalizedRoute = routeNumber.Replace("-", "");

            // Also try with hyphen inserted before last 2 digits (e.g., JAAJ17 -> JAAJ-17)
            var routeWithHyphen = normalizedRoute.Length > 2
                ? normalizedRoute.Insert(normalizedRoute.Length - 2, "-")
                : normalizedRoute;

            return await _context.Orders
                .Include(o => o.PlannedItems)
                .Where(o =>
                    (o.PlannedRoute == normalizedRoute ||
                     o.PlannedRoute == routeWithHyphen ||
                     o.PlannedRoute == routeNumber) &&
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

    // ===== SESSION OPERATIONS =====

    /// <summary>
    /// Get active session for a route (status = "active")
    /// </summary>
    public async Task<ShipmentLoadSession?> GetActiveSessionByRouteAsync(string routeNumber)
    {
        try
        {
            return await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .FirstOrDefaultAsync(s =>
                    s.RouteNumber == routeNumber &&
                    s.Status == "active");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active session for route: {RouteNumber}", routeNumber);
            throw;
        }
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    public async Task<ShipmentLoadSession?> GetSessionByIdAsync(Guid sessionId)
    {
        try
        {
            return await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Create new shipment load session
    /// </summary>
    public async Task<ShipmentLoadSession> CreateSessionAsync(ShipmentLoadSession session)
    {
        try
        {
            _context.ShipmentLoadSessions.Add(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created shipment load session: {SessionId} for route: {RouteNumber}",
                session.SessionId, session.RouteNumber);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for route: {RouteNumber}", session.RouteNumber);
            throw;
        }
    }

    /// <summary>
    /// Update existing session
    /// </summary>
    public async Task<ShipmentLoadSession> UpdateSessionAsync(ShipmentLoadSession session)
    {
        try
        {
            _context.ShipmentLoadSessions.Update(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated shipment load session: {SessionId}", session.SessionId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session: {SessionId}", session.SessionId);
            throw;
        }
    }

    /// <summary>
    /// Get session with all linked orders and their skid scans
    /// </summary>
    public async Task<ShipmentLoadSession?> GetSessionWithOrdersAsync(Guid sessionId)
    {
        try
        {
            var session = await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
                return null;

            // Get orders linked to this session
            var orders = await _context.Orders
                .Include(o => o.PlannedItems)
                    .ThenInclude(pi => pi.SkidScans)
                .Where(o => o.ShipmentLoadSessionId == sessionId)
                .ToListAsync();

            // Attach orders to session for convenience (not a navigation property)
            // The session object will be returned and the caller can query orders separately

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session with orders: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get all exceptions for a session
    /// </summary>
    public async Task<List<ShipmentLoadException>> GetSessionExceptionsAsync(Guid sessionId)
    {
        try
        {
            return await _context.ShipmentLoadExceptions
                .Where(e => e.SessionId == sessionId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exceptions for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get all orders linked to a session
    /// </summary>
    public async Task<List<Order>> GetOrdersBySessionIdAsync(Guid sessionId)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                    .ThenInclude(pi => pi.SkidScans)
                .Where(o => o.ShipmentLoadSessionId == sessionId)
                .OrderBy(o => o.RealOrderNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Link order to session (set ShipmentLoadSessionId)
    /// </summary>
    public async Task LinkOrderToSessionAsync(Guid orderId, Guid sessionId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order not found: {orderId}");

            order.ShipmentLoadSessionId = sessionId;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Linked order {OrderId} to session {SessionId}", orderId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking order {OrderId} to session {SessionId}", orderId, sessionId);
            throw;
        }
    }

    // ===== SKID OPERATIONS =====

    /// <summary>
    /// Get all skid scans for an order
    /// </summary>
    public async Task<List<SkidScan>> GetSkidScansByOrderIdAsync(Guid orderId)
    {
        try
        {
            // Get all PlannedItemIds for this order
            var plannedItemIds = await _context.PlannedItems
                .Where(pi => pi.OrderId == orderId)
                .Select(pi => pi.PlannedItemId)
                .ToListAsync();

            if (!plannedItemIds.Any())
                return new List<SkidScan>();

            // Get all skid scans for these planned items
            return await _context.SkidScans
                .Where(s => plannedItemIds.Contains(s.PlannedItemId))
                .OrderBy(s => s.SkidNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skid scans for order: {OrderId}", orderId);
            throw;
        }
    }

    // ===== EXCEPTION OPERATIONS =====

    /// <summary>
    /// Add exception to session
    /// </summary>
    public async Task<ShipmentLoadException> AddExceptionAsync(ShipmentLoadException exception)
    {
        try
        {
            _context.ShipmentLoadExceptions.Add(exception);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added exception {ExceptionType} to session {SessionId}",
                exception.ExceptionType, exception.SessionId);
            return exception;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exception to session: {SessionId}", exception.SessionId);
            throw;
        }
    }

    /// <summary>
    /// Delete exception by ID
    /// </summary>
    public async Task DeleteExceptionAsync(Guid exceptionId)
    {
        try
        {
            var exception = await _context.ShipmentLoadExceptions.FindAsync(exceptionId);
            if (exception != null)
            {
                _context.ShipmentLoadExceptions.Remove(exception);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted exception: {ExceptionId}", exceptionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exception: {ExceptionId}", exceptionId);
            throw;
        }
    }
}
