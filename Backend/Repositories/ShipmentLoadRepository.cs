// Author: Hassan
// Date: 2025-12-08
// Updated: 2025-12-24 - Added GetSkidBuildExceptionsByOrderIdAsync to support skid build exceptions in shipment load
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
    Task<List<Order>> GetAllOrdersByRouteAsync(string routeNumber); // All orders regardless of status
    Task<Order?> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode);
    Task<int> GetSkidScansCountForOrderAsync(Guid orderId);
    Task UpdateOrdersAsync(List<Order> orders);
    Task<List<Order>> GetOrdersByRouteAndStatusAsync(string routeNumber, OrderStatus status);
    Task<List<Order>> GetOrdersBySessionIdAsync(Guid sessionId);
    Task LinkOrderToSessionAsync(Guid orderId, Guid sessionId);

    // Skid operations
    Task<List<SkidScan>> GetSkidScansByOrderIdAsync(Guid orderId);
    Task<SkidScan?> GetSkidScanByRawSkidIdAsync(Guid orderId, string rawSkidId, string? palletizationCode = null);
    Task UpdateSkidScanAsync(SkidScan skidScan);

    // Exception operations
    Task<ShipmentLoadException> AddExceptionAsync(ShipmentLoadException exception);
    Task DeleteExceptionAsync(Guid exceptionId);
    Task<int> DeleteExceptionsBySessionIdAsync(Guid sessionId);

    // Skid Build Exception operations
    Task<List<SkidBuildException>> GetSkidBuildExceptionsByOrderIdAsync(Guid orderId);

    // Pre-Shipment operations
    Task<string?> GetRouteByOrderNumberAsync(string orderNumber, string dockCode);
    Task<ShipmentLoadSession?> GetSessionByRouteAndCreatedViaAsync(string routeNumber, string createdVia);
    Task<List<ShipmentLoadSession>> GetSessionsByCreatedViaAsync(string createdVia);

    // Restart operations
    Task<int> ClearShipmentLoadSessionIdForSessionAsync(Guid sessionId);
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
    /// Get ALL orders for a route regardless of status
    /// Used to validate all orders on route have completed skid build before shipment
    /// Normalizes route comparison by removing hyphens (e.g., JAAJ17 matches JAAJ-17)
    /// </summary>
    public async Task<List<Order>> GetAllOrdersByRouteAsync(string routeNumber)
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
                .Where(o =>
                    o.PlannedRoute == normalizedRoute ||
                    o.PlannedRoute == routeWithHyphen ||
                    o.PlannedRoute == routeNumber)
                .OrderBy(o => o.RealOrderNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders for route: {RouteNumber}", routeNumber);
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
    /// Get resumable session for a route (status = "active" or "error")
    /// Sessions with "error" status can be resumed so users can retry after Toyota API failures
    /// </summary>
    public async Task<ShipmentLoadSession?> GetActiveSessionByRouteAsync(string routeNumber)
    {
        try
        {
            // Find the most recent session that can be resumed (active or error)
            // This allows users to retry after Toyota API failures without losing their work
            return await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .Where(s =>
                    s.RouteNumber == routeNumber &&
                    (s.Status == "active" || s.Status == "error"))
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resumable session for route: {RouteNumber}", routeNumber);
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

    /// <summary>
    /// Get a specific skid scan by order ID, raw skid ID, and palletization code
    /// IMPORTANT: Must match ALL three fields for exact skid identification
    /// Within the same order, the same RawSkidId can appear with different PalletizationCode
    /// </summary>
    public async Task<SkidScan?> GetSkidScanByRawSkidIdAsync(Guid orderId, string rawSkidId, string? palletizationCode = null)
    {
        try
        {
            // Get all PlannedItemIds for this order
            var plannedItemIds = await _context.PlannedItems
                .Where(pi => pi.OrderId == orderId)
                .Select(pi => pi.PlannedItemId)
                .ToListAsync();

            if (!plannedItemIds.Any())
                return null;

            // Find the skid scan matching the raw skid ID AND palletization code
            // CRITICAL: Both RawSkidId and PalletizationCode must match for unique identification
            var query = _context.SkidScans
                .Where(s => plannedItemIds.Contains(s.PlannedItemId) && s.RawSkidId == rawSkidId);

            // Add palletization code filter if provided
            if (!string.IsNullOrWhiteSpace(palletizationCode))
            {
                query = query.Where(s => s.PalletizationCode == palletizationCode);
            }

            return await query.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skid scan for order {OrderId}, RawSkidId {RawSkidId}, PalletizationCode {PalletizationCode}",
                orderId, rawSkidId, palletizationCode ?? "NULL");
            throw;
        }
    }

    /// <summary>
    /// Update a skid scan record
    /// </summary>
    public async Task UpdateSkidScanAsync(SkidScan skidScan)
    {
        try
        {
            skidScan.UpdatedAt = DateTime.UtcNow;
            _context.SkidScans.Update(skidScan);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated SkidScan {ScanId} - ShipmentLoadSessionId: {SessionId}",
                skidScan.ScanId, skidScan.ShipmentLoadSessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skid scan: {ScanId}", skidScan.ScanId);
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

    /// <summary>
    /// Get all skid build exceptions for an order
    /// These are exceptions recorded during skid build that need to be included in shipment load
    /// </summary>
    public async Task<List<SkidBuildException>> GetSkidBuildExceptionsByOrderIdAsync(Guid orderId)
    {
        try
        {
            return await _context.SkidBuildExceptions
                .Where(e => e.OrderId == orderId)
                .OrderBy(e => e.SkidNumber)
                .ThenBy(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skid build exceptions for order: {OrderId}", orderId);
            throw;
        }
    }

    // ===== PRE-SHIPMENT OPERATIONS =====

    /// <summary>
    /// Get route number by order number and dock code
    /// Used in Pre-Shipment to determine route from manifest scan
    /// </summary>
    public async Task<string?> GetRouteByOrderNumberAsync(string orderNumber, string dockCode)
    {
        try
        {
            var order = await _context.Orders
                .Where(o => o.RealOrderNumber == orderNumber && o.DockCode == dockCode)
                .Select(o => o.PlannedRoute)
                .FirstOrDefaultAsync();

            _logger.LogInformation("Route lookup - OrderNumber: {OrderNumber}, DockCode: {DockCode}, PlannedRoute: {PlannedRoute}",
                orderNumber, dockCode, order ?? "NULL");

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route for order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Get session by route number and CreatedVia field
    /// Used to find existing Pre-Shipment sessions or Shipment Load sessions
    /// </summary>
    public async Task<ShipmentLoadSession?> GetSessionByRouteAndCreatedViaAsync(string routeNumber, string createdVia)
    {
        try
        {
            // Normalize route for comparison
            var normalizedRoute = routeNumber.Replace("-", "");
            var routeWithHyphen = normalizedRoute.Length > 2
                ? normalizedRoute.Insert(normalizedRoute.Length - 2, "-")
                : normalizedRoute;

            return await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .Where(s =>
                    (s.RouteNumber == routeNumber ||
                     s.RouteNumber == normalizedRoute ||
                     s.RouteNumber == routeWithHyphen) &&
                    s.CreatedVia == createdVia &&
                    (s.Status == "active" || s.Status == "error"))
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session for route: {RouteNumber}, CreatedVia: {CreatedVia}",
                routeNumber, createdVia);
            throw;
        }
    }

    /// <summary>
    /// Get all sessions by CreatedVia field
    /// Used to get all Pre-Shipment sessions or Shipment Load sessions
    /// </summary>
    public async Task<List<ShipmentLoadSession>> GetSessionsByCreatedViaAsync(string createdVia)
    {
        try
        {
            return await _context.ShipmentLoadSessions
                .Include(s => s.ShipmentLoadExceptions)
                .Where(s => s.CreatedVia == createdVia)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for CreatedVia: {CreatedVia}", createdVia);
            throw;
        }
    }

    /// <summary>
    /// Delete all exceptions for a session (used during session restart)
    /// </summary>
    public async Task<int> DeleteExceptionsBySessionIdAsync(Guid sessionId)
    {
        try
        {
            var exceptions = await _context.ShipmentLoadExceptions
                .Where(e => e.SessionId == sessionId)
                .ToListAsync();

            if (!exceptions.Any())
            {
                _logger.LogInformation("No exceptions found for session: {SessionId}", sessionId);
                return 0;
            }

            _context.ShipmentLoadExceptions.RemoveRange(exceptions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} exceptions for session: {SessionId}", exceptions.Count, sessionId);
            return exceptions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exceptions for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Clear ShipmentLoadSessionId from all SkidScans for a session (used during session restart)
    /// </summary>
    public async Task<int> ClearShipmentLoadSessionIdForSessionAsync(Guid sessionId)
    {
        try
        {
            var scans = await _context.SkidScans
                .Where(s => s.ShipmentLoadSessionId == sessionId)
                .ToListAsync();

            if (!scans.Any())
            {
                _logger.LogInformation("No scans found for session: {SessionId}", sessionId);
                return 0;
            }

            foreach (var scan in scans)
            {
                scan.ShipmentLoadSessionId = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared ShipmentLoadSessionId from {Count} scans for session: {SessionId}",
                scans.Count, sessionId);
            return scans.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing ShipmentLoadSessionId for session: {SessionId}", sessionId);
            throw;
        }
    }
}
