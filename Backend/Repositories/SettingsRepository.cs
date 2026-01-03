// Author: Hassan
// Date: 2025-12-01
// Updated: 2026-01-03 - Migrated to use SiteSettings instead of separate settings tables
// Description: Repository for Settings entities - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Settings repository operations
/// Now uses consolidated SiteSettings table
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Get site settings (single row for entire deployment)
    /// </summary>
    Task<SiteSettings?> GetSiteSettingsAsync();

    /// <summary>
    /// Save site settings
    /// </summary>
    Task<SiteSettings> SaveSiteSettingsAsync(SiteSettings settings);
}

/// <summary>
/// Repository implementation for Settings entities
/// Uses consolidated SiteSettings table
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(VuteqDbContext context, ILogger<SettingsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get site settings (global settings - only one record per deployment)
    /// </summary>
    public async Task<SiteSettings?> GetSiteSettingsAsync()
    {
        try
        {
            return await _context.SiteSettings
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site settings");
            throw;
        }
    }

    /// <summary>
    /// Save site settings (create if not exists, update if exists)
    /// </summary>
    public async Task<SiteSettings> SaveSiteSettingsAsync(SiteSettings settings)
    {
        try
        {
            var existing = await _context.SiteSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new settings
                settings.SettingId = Guid.NewGuid();
                settings.CreatedAt = DateTime.UtcNow;
                _context.SiteSettings.Add(settings);
                await _context.SaveChangesAsync();
                return settings;
            }
            else
            {
                // Update existing settings - Site tab
                existing.PlantLocation = settings.PlantLocation;
                existing.PlantOpeningTime = settings.PlantOpeningTime;
                existing.PlantClosingTime = settings.PlantClosingTime;
                existing.EnablePreShipmentScan = settings.EnablePreShipmentScan;

                // Update existing settings - Dock Monitor tab
                existing.DockBehindThreshold = settings.DockBehindThreshold;
                existing.DockCriticalThreshold = settings.DockCriticalThreshold;
                existing.DockDisplayMode = settings.DockDisplayMode;
                existing.DockRefreshInterval = settings.DockRefreshInterval;
                existing.DockOrderLookbackHours = settings.DockOrderLookbackHours;

                // Update existing settings - Internal Kanban tab
                existing.KanbanAllowDuplicates = settings.KanbanAllowDuplicates;
                existing.KanbanDuplicateWindowHours = settings.KanbanDuplicateWindowHours;
                existing.KanbanAlertOnDuplicate = settings.KanbanAlertOnDuplicate;

                // Audit
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = settings.UpdatedBy;

                _context.SiteSettings.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving site settings");
            throw;
        }
    }
}
