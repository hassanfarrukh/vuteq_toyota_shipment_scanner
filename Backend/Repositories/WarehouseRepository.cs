// Author: Hassan
// Date: 2025-11-24
// Description: Repository for Warehouse entity - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Warehouse repository operations
/// </summary>
public interface IWarehouseRepository
{
    Task<IEnumerable<WarehouseMaster>> GetAllActiveAsync();
    Task<IEnumerable<WarehouseMaster>> GetAllAsync();
    Task<WarehouseMaster?> GetByIdAsync(Guid warehouseId);
    Task<WarehouseMaster?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code);
    Task<WarehouseMaster> CreateAsync(WarehouseMaster warehouse);
    Task<WarehouseMaster> UpdateAsync(WarehouseMaster warehouse);
    Task<bool> DeleteAsync(Guid warehouseId);
}

/// <summary>
/// Repository implementation for Warehouse entity
/// </summary>
public class WarehouseRepository : IWarehouseRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<WarehouseRepository> _logger;

    public WarehouseRepository(VuteqDbContext context, ILogger<WarehouseRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active warehouses
    /// </summary>
    public async Task<IEnumerable<WarehouseMaster>> GetAllActiveAsync()
    {
        try
        {
            return await _context.WarehouseMasters
                .Where(w => w.IsActive)
                .OrderBy(w => w.Code)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active warehouses");
            throw;
        }
    }

    /// <summary>
    /// Get all warehouses (active and inactive)
    /// </summary>
    public async Task<IEnumerable<WarehouseMaster>> GetAllAsync()
    {
        try
        {
            return await _context.WarehouseMasters
                .OrderBy(w => w.Code)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all warehouses");
            throw;
        }
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    public async Task<WarehouseMaster?> GetByIdAsync(Guid warehouseId)
    {
        try
        {
            return await _context.WarehouseMasters
                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse by ID: {WarehouseId}", warehouseId);
            throw;
        }
    }

    /// <summary>
    /// Get warehouse by code
    /// </summary>
    public async Task<WarehouseMaster?> GetByCodeAsync(string code)
    {
        try
        {
            return await _context.WarehouseMasters
                .FirstOrDefaultAsync(w => w.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse by code: {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Check if warehouse code already exists
    /// </summary>
    public async Task<bool> CodeExistsAsync(string code)
    {
        try
        {
            return await _context.WarehouseMasters
                .AnyAsync(w => w.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking warehouse code existence: {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    public async Task<WarehouseMaster> CreateAsync(WarehouseMaster warehouse)
    {
        try
        {
            _context.WarehouseMasters.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse: {Code}", warehouse.Code);
            throw;
        }
    }

    /// <summary>
    /// Update an existing warehouse
    /// </summary>
    public async Task<WarehouseMaster> UpdateAsync(WarehouseMaster warehouse)
    {
        try
        {
            _context.WarehouseMasters.Update(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", warehouse.WarehouseId);
            throw;
        }
    }

    /// <summary>
    /// Delete a warehouse (soft delete by setting IsActive = false)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid warehouseId)
    {
        try
        {
            var warehouse = await GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return false;
            }

            warehouse.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", warehouseId);
            throw;
        }
    }
}
