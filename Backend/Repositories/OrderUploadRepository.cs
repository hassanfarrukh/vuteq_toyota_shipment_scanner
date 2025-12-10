// Author: Hassan
// Date: 2025-12-01
// Description: Repository for OrderUpload entity - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for OrderUpload repository operations
/// </summary>
public interface IOrderUploadRepository
{
    Task<OrderUpload> CreateUploadAsync(OrderUpload upload);
    Task<IEnumerable<OrderUpload>> GetAllUploadsAsync();
    Task<OrderUpload?> GetUploadByIdAsync(Guid id);
    Task<OrderUpload> UpdateUploadStatusAsync(Guid id, string status, string? errorMessage = null, int ordersCreated = 0, int totalItemsCreated = 0);
    Task<OrderUpload> UpdateUploadAsync(OrderUpload upload);
    Task<bool> DeleteUploadAsync(Guid id);
}

/// <summary>
/// Repository implementation for OrderUpload entity
/// </summary>
public class OrderUploadRepository : IOrderUploadRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<OrderUploadRepository> _logger;

    public OrderUploadRepository(VuteqDbContext context, ILogger<OrderUploadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order upload record
    /// </summary>
    public async Task<OrderUpload> CreateUploadAsync(OrderUpload upload)
    {
        try
        {
            _context.OrderUploads.Add(upload);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order upload record created: {UploadId}", upload.Id);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order upload record");
            throw;
        }
    }

    /// <summary>
    /// Get all order uploads, ordered by upload date descending
    /// </summary>
    public async Task<IEnumerable<OrderUpload>> GetAllUploadsAsync()
    {
        try
        {
            return await _context.OrderUploads
                .Include(u => u.UploadedByUser)
                .OrderByDescending(u => u.UploadDate)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order uploads");
            throw;
        }
    }

    /// <summary>
    /// Get order upload by ID
    /// </summary>
    public async Task<OrderUpload?> GetUploadByIdAsync(Guid id)
    {
        try
        {
            return await _context.OrderUploads
                .Include(u => u.UploadedByUser)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order upload: {UploadId}", id);
            throw;
        }
    }

    /// <summary>
    /// Update order upload status
    /// </summary>
    public async Task<OrderUpload> UpdateUploadStatusAsync(Guid id, string status, string? errorMessage = null, int ordersCreated = 0, int totalItemsCreated = 0)
    {
        try
        {
            var upload = await _context.OrderUploads.FindAsync(id);
            if (upload == null)
            {
                throw new InvalidOperationException($"Order upload with ID {id} not found");
            }

            upload.Status = status;
            if (errorMessage != null)
            {
                upload.ErrorMessage = errorMessage;
            }
            upload.OrdersCreated = ordersCreated;
            upload.TotalItemsCreated = totalItemsCreated;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Order upload status updated: {UploadId} -> {Status} ({OrdersCreated} orders, {ItemsCreated} items)",
                id, status, ordersCreated, totalItemsCreated);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order upload status: {UploadId}", id);
            throw;
        }
    }

    /// <summary>
    /// Update order upload record (for Excel files with NAMC summary)
    /// </summary>
    public async Task<OrderUpload> UpdateUploadAsync(OrderUpload upload)
    {
        try
        {
            _context.OrderUploads.Update(upload);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order upload updated: {UploadId} -> {Status} ({OrdersCreated} orders, {ItemsCreated} items)",
                upload.Id, upload.Status, upload.OrdersCreated, upload.TotalItemsCreated);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order upload: {UploadId}", upload.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete order upload record
    /// </summary>
    public async Task<bool> DeleteUploadAsync(Guid id)
    {
        try
        {
            var upload = await _context.OrderUploads.FindAsync(id);
            if (upload == null)
            {
                return false;
            }

            _context.OrderUploads.Remove(upload);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order upload deleted: {UploadId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order upload: {UploadId}", id);
            throw;
        }
    }
}
