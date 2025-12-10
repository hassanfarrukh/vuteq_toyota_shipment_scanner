// Author: Hassan
// Date: 2025-11-24
// Description: Repository for Office entity - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Office repository operations
/// </summary>
public interface IOfficeRepository
{
    Task<IEnumerable<OfficeMaster>> GetAllActiveAsync();
    Task<IEnumerable<OfficeMaster>> GetAllAsync();
    Task<OfficeMaster?> GetByIdAsync(Guid officeId);
    Task<OfficeMaster?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code);
    Task<OfficeMaster> CreateAsync(OfficeMaster office);
    Task<OfficeMaster> UpdateAsync(OfficeMaster office);
    Task<bool> DeleteAsync(Guid officeId);
    Task<bool> HasDependentWarehousesAsync(string officeCode);
}

/// <summary>
/// Repository implementation for Office entity
/// </summary>
public class OfficeRepository : IOfficeRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<OfficeRepository> _logger;

    public OfficeRepository(VuteqDbContext context, ILogger<OfficeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active offices
    /// </summary>
    public async Task<IEnumerable<OfficeMaster>> GetAllActiveAsync()
    {
        try
        {
            return await _context.OfficeMasters
                .Where(o => o.IsActive)
                .OrderBy(o => o.Code)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active offices");
            throw;
        }
    }

    /// <summary>
    /// Get all offices (active and inactive)
    /// </summary>
    public async Task<IEnumerable<OfficeMaster>> GetAllAsync()
    {
        try
        {
            return await _context.OfficeMasters
                .OrderBy(o => o.Code)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all offices");
            throw;
        }
    }

    /// <summary>
    /// Get office by ID
    /// </summary>
    public async Task<OfficeMaster?> GetByIdAsync(Guid officeId)
    {
        try
        {
            return await _context.OfficeMasters
                .FirstOrDefaultAsync(o => o.OfficeId == officeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving office by ID: {OfficeId}", officeId);
            throw;
        }
    }

    /// <summary>
    /// Get office by code
    /// </summary>
    public async Task<OfficeMaster?> GetByCodeAsync(string code)
    {
        try
        {
            return await _context.OfficeMasters
                .FirstOrDefaultAsync(o => o.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving office by code: {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Check if office code already exists
    /// </summary>
    public async Task<bool> CodeExistsAsync(string code)
    {
        try
        {
            return await _context.OfficeMasters
                .AnyAsync(o => o.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking office code existence: {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Create a new office
    /// </summary>
    public async Task<OfficeMaster> CreateAsync(OfficeMaster office)
    {
        try
        {
            _context.OfficeMasters.Add(office);
            await _context.SaveChangesAsync();
            return office;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating office: {Code}", office.Code);
            throw;
        }
    }

    /// <summary>
    /// Update an existing office
    /// </summary>
    public async Task<OfficeMaster> UpdateAsync(OfficeMaster office)
    {
        try
        {
            _context.OfficeMasters.Update(office);
            await _context.SaveChangesAsync();
            return office;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating office: {OfficeId}", office.OfficeId);
            throw;
        }
    }

    /// <summary>
    /// Delete an office (soft delete by setting IsActive = false)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid officeId)
    {
        try
        {
            var office = await GetByIdAsync(officeId);
            if (office == null)
            {
                return false;
            }

            office.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting office: {OfficeId}", officeId);
            throw;
        }
    }

    /// <summary>
    /// Check if office has dependent warehouses
    /// </summary>
    public async Task<bool> HasDependentWarehousesAsync(string officeCode)
    {
        try
        {
            return await _context.WarehouseMasters
                .AnyAsync(w => w.OfficeCode == officeCode && w.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking dependent warehouses for office: {OfficeCode}", officeCode);
            throw;
        }
    }
}
