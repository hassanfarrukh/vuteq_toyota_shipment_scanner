// Author: Hassan
// Date: 2025-12-01
// Description: Repository for Settings entities - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Repositories;

/// <summary>
/// Interface for Settings repository operations
/// </summary>
public interface ISettingsRepository
{
    // Internal Kanban Settings
    Task<InternalKanbanSetting?> GetInternalKanbanSettingsAsync();
    Task<InternalKanbanSetting> SaveInternalKanbanSettingsAsync(InternalKanbanSetting settings);

    // Dock Monitor Settings (Global - system-wide)
    Task<DockMonitorSetting?> GetDockMonitorSettingsAsync();
    Task<DockMonitorSetting> SaveDockMonitorSettingsAsync(DockMonitorSetting settings);
}

/// <summary>
/// Repository implementation for Settings entities
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

    #region Internal Kanban Settings

    /// <summary>
    /// Get internal kanban settings (global settings - only one record)
    /// </summary>
    public async Task<InternalKanbanSetting?> GetInternalKanbanSettingsAsync()
    {
        try
        {
            return await _context.InternalKanbanSettings
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban settings");
            throw;
        }
    }

    /// <summary>
    /// Save internal kanban settings (create if not exists, update if exists)
    /// </summary>
    public async Task<InternalKanbanSetting> SaveInternalKanbanSettingsAsync(InternalKanbanSetting settings)
    {
        try
        {
            var existing = await _context.InternalKanbanSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new settings
                settings.SettingId = Guid.NewGuid();
                settings.CreatedAt = DateTime.UtcNow;
                _context.InternalKanbanSettings.Add(settings);
            }
            else
            {
                // Update existing settings
                existing.AllowDuplicates = settings.AllowDuplicates;
                existing.DuplicateWindow = settings.DuplicateWindow;
                existing.AlertOnDuplicate = settings.AlertOnDuplicate;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.InternalKanbanSettings.Update(existing);
            }

            await _context.SaveChangesAsync();

            // Return the saved settings
            return existing ?? settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving internal kanban settings");
            throw;
        }
    }

    #endregion

    #region Dock Monitor Settings

    /// <summary>
    /// Get global dock monitor settings (system-wide, not per-user)
    /// </summary>
    public async Task<DockMonitorSetting?> GetDockMonitorSettingsAsync()
    {
        try
        {
            // Get the first/only global settings record
            return await _context.DockMonitorSettings
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving global dock monitor settings");
            throw;
        }
    }

    /// <summary>
    /// Save global dock monitor settings (create if not exists, update if exists)
    /// This is a system-wide setting - only one record should exist
    /// </summary>
    public async Task<DockMonitorSetting> SaveDockMonitorSettingsAsync(DockMonitorSetting settings)
    {
        try
        {
            // Get existing global settings (should be only one record)
            var existing = await _context.DockMonitorSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new global settings
                settings.SettingId = Guid.NewGuid();
                settings.UserId = null; // Global settings have no user
                settings.CreatedAt = DateTime.UtcNow;
                _context.DockMonitorSettings.Add(settings);
            }
            else
            {
                // Update existing global settings
                existing.BehindThreshold = settings.BehindThreshold;
                existing.CriticalThreshold = settings.CriticalThreshold;
                existing.DisplayMode = settings.DisplayMode;
                existing.SelectedLocations = settings.SelectedLocations;
                existing.RefreshInterval = settings.RefreshInterval;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.DockMonitorSettings.Update(existing);
            }

            await _context.SaveChangesAsync();

            // Return the saved settings
            return existing ?? settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving global dock monitor settings");
            throw;
        }
    }

    #endregion
}
