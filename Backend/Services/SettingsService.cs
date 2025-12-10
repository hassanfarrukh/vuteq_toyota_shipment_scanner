// Author: Hassan
// Date: 2025-12-01
// Description: Service for Settings management - handles business logic

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;
using System.Text.Json;

namespace Backend.Services;

/// <summary>
/// Interface for Settings service operations
/// </summary>
public interface ISettingsService
{
    // Internal Kanban Settings
    Task<ApiResponse<InternalKanbanSettingsDto>> GetInternalKanbanSettingsAsync();
    Task<ApiResponse<InternalKanbanSettingsDto>> SaveInternalKanbanSettingsAsync(UpdateInternalKanbanSettingsRequest request);

    // Dock Monitor Settings (Global - system-wide)
    Task<ApiResponse<DockMonitorSettingsDto>> GetDockMonitorSettingsAsync();
    Task<ApiResponse<DockMonitorSettingsDto>> SaveDockMonitorSettingsAsync(UpdateDockMonitorSettingsRequest request);
}

/// <summary>
/// Service implementation for Settings management
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository settingsRepository, ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    #region Internal Kanban Settings

    /// <summary>
    /// Get internal kanban settings (returns defaults if not set)
    /// </summary>
    public async Task<ApiResponse<InternalKanbanSettingsDto>> GetInternalKanbanSettingsAsync()
    {
        try
        {
            var settings = await _settingsRepository.GetInternalKanbanSettingsAsync();

            // If settings don't exist, return default values
            if (settings == null)
            {
                var defaultSettings = new InternalKanbanSettingsDto
                {
                    SettingId = Guid.Empty,
                    AllowDuplicates = false,
                    DuplicateWindowHours = 24,
                    AlertOnDuplicate = true,
                    ModifiedAt = DateTime.UtcNow
                };

                return ApiResponse<InternalKanbanSettingsDto>.SuccessResponse(
                    defaultSettings,
                    "Default internal kanban settings retrieved"
                );
            }

            var dto = MapToInternalKanbanDto(settings);

            return ApiResponse<InternalKanbanSettingsDto>.SuccessResponse(
                dto,
                "Internal kanban settings retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban settings");
            return ApiResponse<InternalKanbanSettingsDto>.ErrorResponse(
                "Failed to retrieve internal kanban settings",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Save internal kanban settings
    /// </summary>
    public async Task<ApiResponse<InternalKanbanSettingsDto>> SaveInternalKanbanSettingsAsync(
        UpdateInternalKanbanSettingsRequest request)
    {
        try
        {
            var settings = new InternalKanbanSetting
            {
                AllowDuplicates = request.AllowDuplicates,
                DuplicateWindow = request.DuplicateWindowHours,
                AlertOnDuplicate = request.AlertOnDuplicate
            };

            var savedSettings = await _settingsRepository.SaveInternalKanbanSettingsAsync(settings);

            _logger.LogInformation("Internal kanban settings saved successfully");

            return ApiResponse<InternalKanbanSettingsDto>.SuccessResponse(
                MapToInternalKanbanDto(savedSettings),
                "Internal kanban settings saved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving internal kanban settings");
            return ApiResponse<InternalKanbanSettingsDto>.ErrorResponse(
                "Failed to save internal kanban settings",
                ex.Message
            );
        }
    }

    #endregion

    #region Dock Monitor Settings

    /// <summary>
    /// Get global dock monitor settings (system-wide, not per-user)
    /// Returns defaults if not set
    /// </summary>
    public async Task<ApiResponse<DockMonitorSettingsDto>> GetDockMonitorSettingsAsync()
    {
        try
        {
            var settings = await _settingsRepository.GetDockMonitorSettingsAsync();

            // If settings don't exist, return default values
            if (settings == null)
            {
                var defaultSettings = new DockMonitorSettingsDto
                {
                    SettingId = Guid.Empty,
                    UserId = null,
                    BehindThreshold = 15,
                    CriticalThreshold = 30,
                    DisplayMode = "FULL",
                    SelectedLocations = new List<string>(),
                    RefreshInterval = 300000, // 5 minutes
                    ModifiedAt = DateTime.UtcNow
                };

                return ApiResponse<DockMonitorSettingsDto>.SuccessResponse(
                    defaultSettings,
                    "Default dock monitor settings retrieved"
                );
            }

            var dto = MapToDockMonitorDto(settings);

            return ApiResponse<DockMonitorSettingsDto>.SuccessResponse(
                dto,
                "Dock monitor settings retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving global dock monitor settings");
            return ApiResponse<DockMonitorSettingsDto>.ErrorResponse(
                "Failed to retrieve dock monitor settings",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Save global dock monitor settings (system-wide, not per-user)
    /// </summary>
    public async Task<ApiResponse<DockMonitorSettingsDto>> SaveDockMonitorSettingsAsync(
        UpdateDockMonitorSettingsRequest request)
    {
        try
        {
            // Validate that critical threshold is greater than behind threshold
            if (request.CriticalThreshold <= request.BehindThreshold)
            {
                return ApiResponse<DockMonitorSettingsDto>.ErrorResponse(
                    "Invalid thresholds",
                    "Critical threshold must be greater than behind threshold"
                );
            }

            var settings = new DockMonitorSetting
            {
                UserId = null, // Global settings have no specific user
                BehindThreshold = request.BehindThreshold,
                CriticalThreshold = request.CriticalThreshold,
                DisplayMode = request.DisplayMode,
                SelectedLocations = JsonSerializer.Serialize(request.SelectedLocations),
                RefreshInterval = 300000 // Default 5 minutes
            };

            var savedSettings = await _settingsRepository.SaveDockMonitorSettingsAsync(settings);

            _logger.LogInformation("Global dock monitor settings saved successfully");

            return ApiResponse<DockMonitorSettingsDto>.SuccessResponse(
                MapToDockMonitorDto(savedSettings),
                "Dock monitor settings saved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving global dock monitor settings");
            return ApiResponse<DockMonitorSettingsDto>.ErrorResponse(
                "Failed to save dock monitor settings",
                ex.Message
            );
        }
    }

    #endregion

    #region Mapping Methods

    /// <summary>
    /// Map InternalKanbanSetting entity to DTO
    /// </summary>
    private static InternalKanbanSettingsDto MapToInternalKanbanDto(InternalKanbanSetting settings)
    {
        return new InternalKanbanSettingsDto
        {
            SettingId = settings.SettingId,
            AllowDuplicates = settings.AllowDuplicates,
            DuplicateWindowHours = settings.DuplicateWindow,
            AlertOnDuplicate = settings.AlertOnDuplicate,
            ModifiedAt = settings.UpdatedAt ?? settings.CreatedAt
        };
    }

    /// <summary>
    /// Map DockMonitorSetting entity to DTO
    /// </summary>
    private static DockMonitorSettingsDto MapToDockMonitorDto(DockMonitorSetting settings)
    {
        List<string> locations = new List<string>();

        // Parse JSON array from SelectedLocations
        if (!string.IsNullOrEmpty(settings.SelectedLocations))
        {
            try
            {
                locations = JsonSerializer.Deserialize<List<string>>(settings.SelectedLocations) ?? new List<string>();
            }
            catch
            {
                // If parsing fails, return empty list
                locations = new List<string>();
            }
        }

        return new DockMonitorSettingsDto
        {
            SettingId = settings.SettingId,
            UserId = settings.UserId,
            BehindThreshold = settings.BehindThreshold,
            CriticalThreshold = settings.CriticalThreshold,
            DisplayMode = settings.DisplayMode,
            SelectedLocations = locations,
            RefreshInterval = settings.RefreshInterval,
            ModifiedAt = settings.UpdatedAt ?? settings.CreatedAt
        };
    }

    #endregion
}
