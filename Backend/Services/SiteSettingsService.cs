// Author: Hassan
// Date: 2025-01-03
// Description: Service for Site Settings management - handles business logic for consolidated site-wide settings

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for SiteSettings service operations
/// </summary>
public interface ISiteSettingsService
{
    Task<ApiResponse<SiteSettingsDto>> GetSiteSettingsAsync();
    Task<ApiResponse<SiteSettingsDto>> UpdateSiteSettingsAsync(UpdateSiteSettingsRequest request, Guid? userId);
}

/// <summary>
/// Service implementation for Site Settings management
/// </summary>
public class SiteSettingsService : ISiteSettingsService
{
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly ILogger<SiteSettingsService> _logger;

    public SiteSettingsService(
        ISiteSettingsRepository siteSettingsRepository,
        ILogger<SiteSettingsService> logger)
    {
        _siteSettingsRepository = siteSettingsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get site settings (returns defaults if not set, or creates them)
    /// </summary>
    public async Task<ApiResponse<SiteSettingsDto>> GetSiteSettingsAsync()
    {
        try
        {
            var settings = await _siteSettingsRepository.GetAsync();

            // Settings are always returned (created if not exists)
            if (settings == null)
            {
                return ApiResponse<SiteSettingsDto>.ErrorResponse(
                    "Failed to retrieve site settings",
                    "Settings could not be loaded or created"
                );
            }

            var dto = MapToDto(settings);

            return ApiResponse<SiteSettingsDto>.SuccessResponse(
                dto,
                "Site settings retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site settings");
            return ApiResponse<SiteSettingsDto>.ErrorResponse(
                "Failed to retrieve site settings",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update site settings
    /// </summary>
    public async Task<ApiResponse<SiteSettingsDto>> UpdateSiteSettingsAsync(
        UpdateSiteSettingsRequest request,
        Guid? userId)
    {
        try
        {
            // Validate that critical threshold is greater than behind threshold
            if (request.DockCriticalThreshold <= request.DockBehindThreshold)
            {
                return ApiResponse<SiteSettingsDto>.ErrorResponse(
                    "Invalid thresholds",
                    "Critical threshold must be greater than behind threshold"
                );
            }

            // Validate plant times if both are set
            // Issue #3 Fix: Allow overnight shifts (e.g., 6:30 AM to 4:00 AM next day)
            // Overnight shift = closing time < opening time (e.g., 04:00 < 06:30)
            // This is VALID for overnight operations
            // Only invalid case: opening == closing (zero-length shift)
            if (request.PlantOpeningTime.HasValue && request.PlantClosingTime.HasValue)
            {
                if (request.PlantClosingTime == request.PlantOpeningTime)
                {
                    return ApiResponse<SiteSettingsDto>.ErrorResponse(
                        "Invalid plant hours",
                        "Plant opening and closing times cannot be the same"
                    );
                }
                // Note: Closing < Opening is VALID (overnight shift pattern)
                // Example: Opening 06:30, Closing 04:00 = 21.5 hour shift spanning midnight
            }

            var settings = new SiteSettings
            {
                PlantLocation = request.PlantLocation,
                PlantOpeningTime = request.PlantOpeningTime,
                PlantClosingTime = request.PlantClosingTime,
                EnablePreShipmentScan = request.EnablePreShipmentScan,
                DockBehindThreshold = request.DockBehindThreshold,
                DockCriticalThreshold = request.DockCriticalThreshold,
                DockDisplayMode = request.DockDisplayMode,
                DockRefreshInterval = request.DockRefreshInterval,
                DockOrderLookbackHours = request.DockOrderLookbackHours,
                KanbanAllowDuplicates = request.KanbanAllowDuplicates,
                KanbanDuplicateWindowHours = request.KanbanDuplicateWindowHours,
                KanbanAlertOnDuplicate = request.KanbanAlertOnDuplicate,
                OrderArchiveDays = request.OrderArchiveDays,
                UpdatedBy = userId
            };

            var savedSettings = await _siteSettingsRepository.UpdateAsync(settings);

            _logger.LogInformation("Site settings updated successfully by user {UserId}", userId);

            return ApiResponse<SiteSettingsDto>.SuccessResponse(
                MapToDto(savedSettings),
                "Site settings updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site settings");
            return ApiResponse<SiteSettingsDto>.ErrorResponse(
                "Failed to update site settings",
                ex.Message
            );
        }
    }

    #region Mapping Methods

    /// <summary>
    /// Map SiteSettings entity to DTO
    /// </summary>
    private static SiteSettingsDto MapToDto(SiteSettings settings)
    {
        return new SiteSettingsDto
        {
            SettingId = settings.SettingId,
            PlantLocation = settings.PlantLocation,
            PlantOpeningTime = settings.PlantOpeningTime,
            PlantClosingTime = settings.PlantClosingTime,
            EnablePreShipmentScan = settings.EnablePreShipmentScan,
            DockBehindThreshold = settings.DockBehindThreshold,
            DockCriticalThreshold = settings.DockCriticalThreshold,
            DockDisplayMode = settings.DockDisplayMode,
            DockRefreshInterval = settings.DockRefreshInterval,
            DockOrderLookbackHours = settings.DockOrderLookbackHours,
            KanbanAllowDuplicates = settings.KanbanAllowDuplicates,
            KanbanDuplicateWindowHours = settings.KanbanDuplicateWindowHours,
            KanbanAlertOnDuplicate = settings.KanbanAlertOnDuplicate,
            OrderArchiveDays = settings.OrderArchiveDays,
            ModifiedAt = settings.UpdatedAt ?? settings.CreatedAt
        };
    }

    #endregion
}
