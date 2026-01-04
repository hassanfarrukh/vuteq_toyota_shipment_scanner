// Author: Hassan
// Date: 2025-12-06
// Updated: 2025-12-13 - Added ScanDetails support
// Description: Repository for Skid Build operations - handles data access using EF Core

using Backend.Data;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Skid Build repository operations
/// </summary>
public interface ISkidBuildRepository
{
    // Order operations
    Task<Order?> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task UpdateOrderAsync(Order order);

    // PlannedItem operations
    Task<PlannedItem?> GetPlannedItemByIdAsync(Guid plannedItemId);

    // Session operations
    Task<SkidBuildSession> CreateSessionAsync(SkidBuildSession session);
    Task<SkidBuildSession?> GetSessionByIdAsync(Guid sessionId);
    Task UpdateSessionAsync(SkidBuildSession session);

    // Scan operations
    Task<SkidScan> CreateScanAsync(SkidScan scan);
    Task<IEnumerable<SkidScan>> GetScansBySessionAsync(Guid sessionId);
    Task<int> GetScannedCountByPlannedItemAsync(Guid plannedItemId);
    Task<List<ScanDetailDto>> GetScanDetailsByPlannedItemAsync(Guid plannedItemId);

    // Exception operations
    Task<SkidBuildException> CreateExceptionAsync(SkidBuildException exception);
    Task<IEnumerable<SkidBuildException>> GetExceptionsBySessionAsync(Guid sessionId);
    Task<IEnumerable<SkidBuildException>> GetExceptionsByOrderAsync(Guid orderId);
    Task<SkidBuildException?> GetExceptionByIdAsync(Guid exceptionId);
    Task<bool> DeleteExceptionAsync(Guid exceptionId);
    Task<int> DeleteExceptionsByOrderIdAsync(Guid orderId);

    // Restart operations
    Task<int> DeleteSkidScansByPlannedItemIdsAsync(List<Guid> plannedItemIds);

    // Duplicate check operations
    Task<bool> IsInternalKanbanAlreadyScannedAsync(Guid orderId, string internalKanban);
    Task<bool> IsToyotaKanbanAlreadyScannedAsync(Guid orderId, Guid plannedItemId, int boxNumber);
}

/// <summary>
/// Repository implementation for Skid Build operations
/// </summary>
public class SkidBuildRepository : ISkidBuildRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<SkidBuildRepository> _logger;

    public SkidBuildRepository(VuteqDbContext context, ILogger<SkidBuildRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Order Operations

    /// <summary>
    /// Get order by order number and dock code with planned items
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
    /// Get order by ID with planned items
    /// </summary>
    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order by ID: {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Get PlannedItem by ID (for palletization validation - GAP-015)
    /// </summary>
    public async Task<PlannedItem?> GetPlannedItemByIdAsync(Guid plannedItemId)
    {
        try
        {
            return await _context.PlannedItems
                .FirstOrDefaultAsync(pi => pi.PlannedItemId == plannedItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving planned item by ID: {PlannedItemId}", plannedItemId);
            throw;
        }
    }

    /// <summary>
    /// Update order (for Toyota API response fields)
    /// </summary>
    public async Task UpdateOrderAsync(Order order)
    {
        try
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order updated: {OrderId}", order.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order: {OrderId}", order.OrderId);
            throw;
        }
    }

    #endregion

    #region Session Operations

    /// <summary>
    /// Create a new skid build session
    /// </summary>
    public async Task<SkidBuildSession> CreateSessionAsync(SkidBuildSession session)
    {
        try
        {
            _context.SkidBuildSessions.Add(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Skid build session created: {SessionId}", session.SessionId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skid build session");
            throw;
        }
    }

    /// <summary>
    /// Get session by ID with related data
    /// </summary>
    public async Task<SkidBuildSession?> GetSessionByIdAsync(Guid sessionId)
    {
        try
        {
            return await _context.SkidBuildSessions
                .Include(s => s.Order)
                    .ThenInclude(o => o!.PlannedItems)
                .Include(s => s.User)
                .Include(s => s.SkidBuildExceptions)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Update session (for completion, status changes)
    /// </summary>
    public async Task UpdateSessionAsync(SkidBuildSession session)
    {
        try
        {
            _context.SkidBuildSessions.Update(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Skid build session updated: {SessionId}", session.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session: {SessionId}", session.SessionId);
            throw;
        }
    }

    #endregion

    #region Scan Operations

    /// <summary>
    /// Create a new skid scan record
    /// </summary>
    public async Task<SkidScan> CreateScanAsync(SkidScan scan)
    {
        try
        {
            _context.SkidScans.Add(scan);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Skid scan created: {ScanId} for PlannedItem: {PlannedItemId}",
                scan.ScanId, scan.PlannedItemId);
            return scan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skid scan");
            throw;
        }
    }

    /// <summary>
    /// Get all scans for a session
    /// </summary>
    public async Task<IEnumerable<SkidScan>> GetScansBySessionAsync(Guid sessionId)
    {
        try
        {
            // Note: SkidScan doesn't have SessionId, so we need to get scans through the order
            var session = await _context.SkidBuildSessions
                .Include(s => s.Order)
                    .ThenInclude(o => o!.PlannedItems)
                        .ThenInclude(pi => pi.SkidScans)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session?.Order?.PlannedItems == null)
                return new List<SkidScan>();

            return session.Order.PlannedItems
                .SelectMany(pi => pi.SkidScans)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scans for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get count of scanned boxes for a planned item
    /// </summary>
    public async Task<int> GetScannedCountByPlannedItemAsync(Guid plannedItemId)
    {
        try
        {
            return await _context.SkidScans
                .Where(s => s.PlannedItemId == plannedItemId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scanned count for planned item: {PlannedItemId}", plannedItemId);
            throw;
        }
    }

    /// <summary>
    /// Get list of scan details for a planned item
    /// Returns all scan details (SkidNumber, BoxNumber, InternalKanban, PalletizationCode) for this planned item
    /// Ordered by SkidNumber, then BoxNumber
    /// </summary>
    public async Task<List<ScanDetailDto>> GetScanDetailsByPlannedItemAsync(Guid plannedItemId)
    {
        try
        {
            return await _context.SkidScans
                .Where(s => s.PlannedItemId == plannedItemId)
                .OrderBy(s => s.SkidNumber)
                .ThenBy(s => s.BoxNumber)
                .Select(s => new ScanDetailDto
                {
                    SkidNumber = s.SkidNumber,
                    BoxNumber = s.BoxNumber,
                    InternalKanban = s.InternalKanban,
                    PalletizationCode = s.PalletizationCode
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scan details for planned item: {PlannedItemId}", plannedItemId);
            throw;
        }
    }

    #endregion

    #region Exception Operations

    /// <summary>
    /// Create a new skid build exception
    /// </summary>
    public async Task<SkidBuildException> CreateExceptionAsync(SkidBuildException exception)
    {
        try
        {
            _context.SkidBuildExceptions.Add(exception);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Skid build exception created: {ExceptionId} with code {ExceptionCode}",
                exception.ExceptionId, exception.ExceptionCode);
            return exception;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skid build exception");
            throw;
        }
    }

    /// <summary>
    /// Get exceptions for a session
    /// </summary>
    public async Task<IEnumerable<SkidBuildException>> GetExceptionsBySessionAsync(Guid sessionId)
    {
        try
        {
            var session = await _context.SkidBuildSessions
                .Include(s => s.SkidBuildExceptions)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            return session?.SkidBuildExceptions ?? new List<SkidBuildException>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exceptions for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get exceptions for an order
    /// </summary>
    public async Task<IEnumerable<SkidBuildException>> GetExceptionsByOrderAsync(Guid orderId)
    {
        try
        {
            return await _context.SkidBuildExceptions
                .Where(e => e.OrderId == orderId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exceptions for order: {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Get exception by ID
    /// </summary>
    public async Task<SkidBuildException?> GetExceptionByIdAsync(Guid exceptionId)
    {
        try
        {
            return await _context.SkidBuildExceptions
                .FirstOrDefaultAsync(e => e.ExceptionId == exceptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exception by ID: {ExceptionId}", exceptionId);
            throw;
        }
    }

    /// <summary>
    /// Delete exception by ID
    /// </summary>
    public async Task<bool> DeleteExceptionAsync(Guid exceptionId)
    {
        try
        {
            var exception = await _context.SkidBuildExceptions
                .FirstOrDefaultAsync(e => e.ExceptionId == exceptionId);

            if (exception == null)
            {
                _logger.LogWarning("Exception not found for deletion: {ExceptionId}", exceptionId);
                return false;
            }

            _context.SkidBuildExceptions.Remove(exception);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Skid build exception deleted: {ExceptionId}", exceptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exception: {ExceptionId}", exceptionId);
            throw;
        }
    }

    /// <summary>
    /// Delete all exceptions for an order (used during session restart)
    /// </summary>
    public async Task<int> DeleteExceptionsByOrderIdAsync(Guid orderId)
    {
        try
        {
            var exceptions = await _context.SkidBuildExceptions
                .Where(e => e.OrderId == orderId)
                .ToListAsync();

            if (!exceptions.Any())
            {
                _logger.LogInformation("No exceptions found for order: {OrderId}", orderId);
                return 0;
            }

            _context.SkidBuildExceptions.RemoveRange(exceptions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} exceptions for order: {OrderId}", exceptions.Count, orderId);
            return exceptions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exceptions for order: {OrderId}", orderId);
            throw;
        }
    }

    #endregion

    #region Restart Operations

    /// <summary>
    /// Delete all SkidScans for a list of PlannedItemIds (used during session restart)
    /// </summary>
    public async Task<int> DeleteSkidScansByPlannedItemIdsAsync(List<Guid> plannedItemIds)
    {
        try
        {
            if (plannedItemIds == null || !plannedItemIds.Any())
            {
                _logger.LogInformation("No planned items provided for scan deletion");
                return 0;
            }

            var scans = await _context.SkidScans
                .Where(s => plannedItemIds.Contains(s.PlannedItemId))
                .ToListAsync();

            if (!scans.Any())
            {
                _logger.LogInformation("No scans found for the provided planned items");
                return 0;
            }

            _context.SkidScans.RemoveRange(scans);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} skid scans for {ItemCount} planned items",
                scans.Count, plannedItemIds.Count);
            return scans.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skid scans for planned items");
            throw;
        }
    }

    #endregion

    #region Duplicate Check Operations

    /// <summary>
    /// Check if an internal kanban has already been scanned for a specific order
    /// </summary>
    public async Task<bool> IsInternalKanbanAlreadyScannedAsync(Guid orderId, string internalKanban)
    {
        try
        {
            // Get all planned items for this order
            var order = await _context.Orders
                .Include(o => o.PlannedItems)
                    .ThenInclude(pi => pi.SkidScans)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order?.PlannedItems == null)
                return false;

            // Check if any scan for this order has the same internal kanban
            var exists = order.PlannedItems
                .SelectMany(pi => pi.SkidScans)
                .Any(scan => !string.IsNullOrWhiteSpace(scan.InternalKanban) &&
                            scan.InternalKanban.Equals(internalKanban, StringComparison.OrdinalIgnoreCase));

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking duplicate internal kanban for order: {OrderId}, kanban: {InternalKanban}",
                orderId, internalKanban);
            throw;
        }
    }

    /// <summary>
    /// Check if a Toyota Kanban (PlannedItemId + BoxNumber combination) has already been scanned for a specific order
    /// STRICT validation - always enforced, no setting override
    /// </summary>
    public async Task<bool> IsToyotaKanbanAlreadyScannedAsync(Guid orderId, Guid plannedItemId, int boxNumber)
    {
        try
        {
            // Get all planned items for this order
            var order = await _context.Orders
                .Include(o => o.PlannedItems)
                    .ThenInclude(pi => pi.SkidScans)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order?.PlannedItems == null)
                return false;

            // Check if any scan for this order has the same PlannedItemId + BoxNumber combination
            var exists = order.PlannedItems
                .SelectMany(pi => pi.SkidScans)
                .Any(scan => scan.PlannedItemId == plannedItemId && scan.BoxNumber == boxNumber);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking duplicate Toyota Kanban for order: {OrderId}, PlannedItemId: {PlannedItemId}, BoxNumber: {BoxNumber}",
                orderId, plannedItemId, boxNumber);
            throw;
        }
    }

    #endregion
}
