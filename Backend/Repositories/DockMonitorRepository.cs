// Author: Hassan
// Date: 2025-12-24
// Description: Repository for Dock Monitor data - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Dock Monitor repository operations
/// </summary>
public interface IDockMonitorRepository
{
    /// <summary>
    /// Get orders from the last 36 hours with shipment session details
    /// </summary>
    Task<List<Order>> GetRecentOrdersWithShipmentsAsync(int hours = 36);

    /// <summary>
    /// Get shipment load sessions from the last 36 hours
    /// </summary>
    Task<List<ShipmentLoadSession>> GetRecentShipmentSessionsAsync(int hours = 36);

    /// <summary>
    /// Get exception types for orders (to detect shortage/projected short)
    /// </summary>
    Task<Dictionary<Guid, List<string>>> GetOrderExceptionTypesAsync(List<Guid> orderIds);
}

/// <summary>
/// Repository implementation for Dock Monitor data
/// </summary>
public class DockMonitorRepository : IDockMonitorRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<DockMonitorRepository> _logger;

    public DockMonitorRepository(VuteqDbContext context, ILogger<DockMonitorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get orders from the last N hours with their shipment session details
    /// Includes orders based on PlannedPickup or CreatedAt within the time window
    /// </summary>
    public async Task<List<Order>> GetRecentOrdersWithShipmentsAsync(int hours = 36)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PlannedPickup >= cutoffTime || o.CreatedAt >= cutoffTime)
                .OrderBy(o => o.PlannedPickup ?? o.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recent orders from last {Hours} hours", orders.Count, hours);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent orders for dock monitor");
            throw;
        }
    }

    /// <summary>
    /// Get shipment load sessions from the last N hours
    /// </summary>
    public async Task<List<ShipmentLoadSession>> GetRecentShipmentSessionsAsync(int hours = 36)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            var sessions = await _context.ShipmentLoadSessions
                .AsNoTracking()
                .Where(s => s.PickupDateTime >= cutoffTime || s.CreatedAt >= cutoffTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recent shipment sessions from last {Hours} hours",
                sessions.Count, hours);
            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent shipment sessions for dock monitor");
            throw;
        }
    }

    /// <summary>
    /// Get exception types for given order IDs (from both skid build and shipment load exceptions)
    /// Returns a dictionary mapping OrderId to list of exception types
    /// </summary>
    public async Task<Dictionary<Guid, List<string>>> GetOrderExceptionTypesAsync(List<Guid> orderIds)
    {
        try
        {
            if (!orderIds.Any())
            {
                return new Dictionary<Guid, List<string>>();
            }

            // Get skid build exceptions
            var skidExceptions = await _context.SkidBuildExceptions
                .AsNoTracking()
                .Where(e => orderIds.Contains(e.OrderId) && !string.IsNullOrEmpty(e.ExceptionCode))
                .Select(e => new { e.OrderId, e.ExceptionCode })
                .ToListAsync();

            // Get shipment load exceptions via sessions
            var shipmentExceptions = await _context.ShipmentLoadExceptions
                .AsNoTracking()
                .Include(e => e.Session)
                .Where(e => !string.IsNullOrEmpty(e.ExceptionType))
                .Select(e => new { e.Session.RouteNumber, e.ExceptionType })
                .ToListAsync();

            // Build dictionary
            var result = new Dictionary<Guid, List<string>>();

            // Add skid build exceptions
            foreach (var exception in skidExceptions)
            {
                if (!result.ContainsKey(exception.OrderId))
                {
                    result[exception.OrderId] = new List<string>();
                }
                if (!string.IsNullOrEmpty(exception.ExceptionCode) &&
                    !result[exception.OrderId].Contains(exception.ExceptionCode))
                {
                    result[exception.OrderId].Add(exception.ExceptionCode);
                }
            }

            // Note: Shipment load exceptions are session-level, not order-level
            // They would need to be matched by route number if needed
            // For now, we're primarily using skid build exceptions

            _logger.LogInformation("Retrieved exception types for {Count} orders", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order exception types");
            throw;
        }
    }
}
