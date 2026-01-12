// Author: Hassan
// Date: 2025-01-03
// Description: Repository for SiteSettings entity - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for SiteSettings repository operations
/// </summary>
public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync();
    Task<SiteSettings> UpdateAsync(SiteSettings settings);
}

/// <summary>
/// Repository implementation for SiteSettings entity
/// </summary>
public class SiteSettingsRepository : ISiteSettingsRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<SiteSettingsRepository> _logger;

    public SiteSettingsRepository(VuteqDbContext context, ILogger<SiteSettingsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get site settings (single row, creates default if not exists)
    /// </summary>
    public async Task<SiteSettings?> GetAsync()
    {
        try
        {
            var settings = await _context.SiteSettings
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // If settings don't exist, create default
            if (settings == null)
            {
                settings = new SiteSettings
                {
                    SettingId = Guid.NewGuid(),
                    PlantLocation = null,
                    PlantOpeningTime = null,
                    PlantClosingTime = null,
                    EnablePreShipmentScan = true,
                    DockBehindThreshold = 15,
                    DockCriticalThreshold = 30,
                    DockDisplayMode = "FULL",
                    DockRefreshInterval = 300000,
                    DockOrderLookbackHours = 36,
                    KanbanAllowDuplicates = false,
                    KanbanDuplicateWindowHours = 24,
                    KanbanAlertOnDuplicate = true,
                    CreatedAt = DateTime.Now
                };

                _context.SiteSettings.Add(settings);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default site settings created");
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site settings");
            throw;
        }
    }

    /// <summary>
    /// Update site settings (create if not exists, update if exists)
    /// </summary>
    public async Task<SiteSettings> UpdateAsync(SiteSettings settings)
    {
        try
        {
            var existing = await _context.SiteSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new settings
                settings.SettingId = Guid.NewGuid();
                settings.CreatedAt = DateTime.Now;
                _context.SiteSettings.Add(settings);
            }
            else
            {
                // Update existing settings
                existing.PlantLocation = settings.PlantLocation;
                existing.PlantOpeningTime = settings.PlantOpeningTime;
                existing.PlantClosingTime = settings.PlantClosingTime;
                existing.EnablePreShipmentScan = settings.EnablePreShipmentScan;
                existing.DockBehindThreshold = settings.DockBehindThreshold;
                existing.DockCriticalThreshold = settings.DockCriticalThreshold;
                existing.DockDisplayMode = settings.DockDisplayMode;
                existing.DockRefreshInterval = settings.DockRefreshInterval;
                existing.DockOrderLookbackHours = settings.DockOrderLookbackHours;
                existing.KanbanAllowDuplicates = settings.KanbanAllowDuplicates;
                existing.KanbanDuplicateWindowHours = settings.KanbanDuplicateWindowHours;
                existing.KanbanAlertOnDuplicate = settings.KanbanAlertOnDuplicate;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = settings.UpdatedBy;
                _context.SiteSettings.Update(existing);
            }

            await _context.SaveChangesAsync();

            // Return the saved settings
            return existing ?? settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site settings");
            throw;
        }
    }
}
