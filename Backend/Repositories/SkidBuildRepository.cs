// Author: Hassan
// Date: 2025-12-06
// Description: Repository for Skid Build operations - handles data access using EF Core

using Backend.Data;
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

    // Session operations
    Task<SkidBuildSession> CreateSessionAsync(SkidBuildSession session);
    Task<SkidBuildSession?> GetSessionByIdAsync(Guid sessionId);
    Task UpdateSessionAsync(SkidBuildSession session);

    // Scan operations
    Task<SkidScan> CreateScanAsync(SkidScan scan);
    Task<IEnumerable<SkidScan>> GetScansBySessionAsync(Guid sessionId);
    Task<int> GetScannedCountByPlannedItemAsync(Guid plannedItemId);

    // Exception operations
    Task<SkidBuildException> CreateExceptionAsync(SkidBuildException exception);
    Task<IEnumerable<SkidBuildException>> GetExceptionsBySessionAsync(Guid sessionId);
    Task<IEnumerable<SkidBuildException>> GetExceptionsByOrderAsync(Guid orderId);
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

    #endregion
}
