// Author: Hassan
// Date: 2025-01-13
// Description: Repository for Internal Kanban Exclusion entity - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Internal Kanban Exclusion repository operations
/// </summary>
public interface IInternalKanbanExclusionRepository
{
    Task<IEnumerable<InternalKanbanExclusion>> GetAllAsync();
    Task<InternalKanbanExclusion?> GetByIdAsync(Guid id);
    Task<InternalKanbanExclusion?> GetByPartNumberAsync(string partNumber);
    Task<bool> IsPartExcludedAsync(string partNumber);
    Task<InternalKanbanExclusion> CreateAsync(InternalKanbanExclusion exclusion);
    Task<List<InternalKanbanExclusion>> CreateBulkAsync(List<InternalKanbanExclusion> exclusions);
    Task<InternalKanbanExclusion> UpdateAsync(InternalKanbanExclusion exclusion);
    Task<bool> DeleteAsync(Guid id);
}

/// <summary>
/// Repository implementation for Internal Kanban Exclusion entity
/// </summary>
public class InternalKanbanExclusionRepository : IInternalKanbanExclusionRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<InternalKanbanExclusionRepository> _logger;

    public InternalKanbanExclusionRepository(VuteqDbContext context, ILogger<InternalKanbanExclusionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all exclusions ordered by part number
    /// </summary>
    public async Task<IEnumerable<InternalKanbanExclusion>> GetAllAsync()
    {
        try
        {
            return await _context.InternalKanbanExclusions
                .OrderBy(e => e.PartNumber)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all internal kanban exclusions");
            throw;
        }
    }

    /// <summary>
    /// Get exclusion by ID
    /// </summary>
    public async Task<InternalKanbanExclusion?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.InternalKanbanExclusions
                .FirstOrDefaultAsync(e => e.ExclusionId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban exclusion by ID: {ExclusionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get exclusion by part number
    /// </summary>
    public async Task<InternalKanbanExclusion?> GetByPartNumberAsync(string partNumber)
    {
        try
        {
            return await _context.InternalKanbanExclusions
                .FirstOrDefaultAsync(e => e.PartNumber == partNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban exclusion by part number: {PartNumber}", partNumber);
            throw;
        }
    }

    /// <summary>
    /// Check if a part number is excluded from internal kanban validation
    /// Returns true if part is excluded (IsExcluded = true)
    /// </summary>
    public async Task<bool> IsPartExcludedAsync(string partNumber)
    {
        try
        {
            return await _context.InternalKanbanExclusions
                .AnyAsync(e => e.PartNumber == partNumber && e.IsExcluded == true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if part is excluded: {PartNumber}", partNumber);
            throw;
        }
    }

    /// <summary>
    /// Create a new exclusion
    /// </summary>
    public async Task<InternalKanbanExclusion> CreateAsync(InternalKanbanExclusion exclusion)
    {
        try
        {
            _context.InternalKanbanExclusions.Add(exclusion);
            await _context.SaveChangesAsync();
            return exclusion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating internal kanban exclusion: {PartNumber}", exclusion.PartNumber);
            throw;
        }
    }

    /// <summary>
    /// Create multiple exclusions in bulk
    /// </summary>
    public async Task<List<InternalKanbanExclusion>> CreateBulkAsync(List<InternalKanbanExclusion> exclusions)
    {
        try
        {
            _context.InternalKanbanExclusions.AddRange(exclusions);
            await _context.SaveChangesAsync();
            return exclusions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk internal kanban exclusions");
            throw;
        }
    }

    /// <summary>
    /// Update an existing exclusion
    /// </summary>
    public async Task<InternalKanbanExclusion> UpdateAsync(InternalKanbanExclusion exclusion)
    {
        try
        {
            _context.InternalKanbanExclusions.Update(exclusion);
            await _context.SaveChangesAsync();
            return exclusion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating internal kanban exclusion: {ExclusionId}", exclusion.ExclusionId);
            throw;
        }
    }

    /// <summary>
    /// Delete an exclusion
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var exclusion = await GetByIdAsync(id);
            if (exclusion == null)
            {
                return false;
            }

            _context.InternalKanbanExclusions.Remove(exclusion);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting internal kanban exclusion: {ExclusionId}", id);
            throw;
        }
    }
}
