// Author: Hassan
// Date: 2025-12-01
// Description: Repository for Order and PlannedItem entities - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Order repository operations
/// </summary>
public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<IEnumerable<PlannedItem>> CreatePlannedItemsAsync(IEnumerable<PlannedItem> items);
    Task<Order?> GetOrderBySeriesAndDockAsync(string orderSeries, string dockCode);
    Task<bool> OrderExistsBySeriesAndDockAsync(string orderSeries, string dockCode);
    Task<bool> OrderExistsBySeriesDockAndNumberAsync(string orderSeries, string dockCode, string orderNumber);
    Task<bool> OrderExistsByOrderNumberAndDockAsync(string realOrderNumber, string dockCode);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<IEnumerable<Order>> GetOrdersByUploadIdAsync(Guid uploadId);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<PlannedItem>> GetAllPlannedItemsWithOrdersAsync();
    Task<IEnumerable<PlannedItem>> GetPlannedItemsByUploadIdAsync(Guid uploadId);
    Task<IEnumerable<PlannedItem>> GetPlannedItemsByOrderIdAsync(Guid orderId);
}

/// <summary>
/// Repository implementation for Order entity
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(VuteqDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    public async Task<Order> CreateOrderAsync(Order order)
    {
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order created: {RealOrderNumber}-{DockCode}", order.RealOrderNumber, order.DockCode);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order: {RealOrderNumber}-{DockCode}", order.RealOrderNumber, order.DockCode);
            throw;
        }
    }

    /// <summary>
    /// Create multiple planned items (bulk insert)
    /// </summary>
    public async Task<IEnumerable<PlannedItem>> CreatePlannedItemsAsync(IEnumerable<PlannedItem> items)
    {
        try
        {
            var itemList = items.ToList();
            _context.PlannedItems.AddRange(itemList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} planned items", itemList.Count);
            return itemList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating planned items");
            throw;
        }
    }

    /// <summary>
    /// Get order by RealOrderNumber and DockCode
    /// </summary>
    public async Task<Order?> GetOrderBySeriesAndDockAsync(string realOrderNumber, string dockCode)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .FirstOrDefaultAsync(o => o.RealOrderNumber == realOrderNumber && o.DockCode == dockCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {RealOrderNumber}-{DockCode}", realOrderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Check if order exists by RealOrderNumber and DockCode
    /// </summary>
    public async Task<bool> OrderExistsBySeriesAndDockAsync(string realOrderNumber, string dockCode)
    {
        try
        {
            return await _context.Orders.AnyAsync(o => o.RealOrderNumber == realOrderNumber && o.DockCode == dockCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking order existence: {RealOrderNumber}-{DockCode}", realOrderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Check if order exists by RealOrderNumber and DockCode (duplicate of above for compatibility)
    /// </summary>
    public async Task<bool> OrderExistsBySeriesDockAndNumberAsync(string realOrderNumber, string dockCode, string orderNumber)
    {
        try
        {
            return await _context.Orders.AnyAsync(o =>
                o.RealOrderNumber == realOrderNumber &&
                o.DockCode == dockCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking order existence: {RealOrderNumber}-{DockCode}",
                realOrderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Check if order exists by RealOrderNumber and DockCode (for Excel uploads)
    /// </summary>
    public async Task<bool> OrderExistsByOrderNumberAndDockAsync(string realOrderNumber, string dockCode)
    {
        try
        {
            return await _context.Orders.AnyAsync(o =>
                o.RealOrderNumber == realOrderNumber &&
                o.DockCode == dockCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking order existence: Order-{RealOrderNumber}-{DockCode}",
                realOrderNumber, dockCode);
            throw;
        }
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .OrderByDescending(o => o.TransmitDate)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            throw;
        }
    }

    /// <summary>
    /// Get orders filtered by upload ID
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByUploadIdAsync(Guid uploadId)
    {
        try
        {
            return await _context.Orders
                .Include(o => o.PlannedItems)
                .Where(o => o.UploadId == uploadId)
                .OrderByDescending(o => o.TransmitDate)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by upload ID: {UploadId}", uploadId);
            throw;
        }
    }

    /// <summary>
    /// Get order by ID
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
    /// Get all planned items with their order information
    /// </summary>
    public async Task<IEnumerable<PlannedItem>> GetAllPlannedItemsWithOrdersAsync()
    {
        try
        {
            return await _context.PlannedItems
                .Include(pi => pi.Order)
                .OrderByDescending(pi => pi.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all planned items with orders");
            throw;
        }
    }

    /// <summary>
    /// Get planned items filtered by upload ID
    /// </summary>
    public async Task<IEnumerable<PlannedItem>> GetPlannedItemsByUploadIdAsync(Guid uploadId)
    {
        try
        {
            return await _context.PlannedItems
                .Include(pi => pi.Order)
                .Where(pi => pi.Order.UploadId == uploadId)
                .OrderByDescending(pi => pi.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving planned items by upload ID: {UploadId}", uploadId);
            throw;
        }
    }

    /// <summary>
    /// Get planned items filtered by order ID
    /// </summary>
    public async Task<IEnumerable<PlannedItem>> GetPlannedItemsByOrderIdAsync(Guid orderId)
    {
        try
        {
            return await _context.PlannedItems
                .Include(pi => pi.Order)
                .Where(pi => pi.OrderId == orderId)
                .OrderByDescending(pi => pi.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving planned items by order ID: {OrderId}", orderId);
            throw;
        }
    }
}
