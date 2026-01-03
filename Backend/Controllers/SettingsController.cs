// Author: Hassan
// Date: 2025-01-03
// Description: Controller for Settings management - provides endpoints for Internal Kanban and Dock Monitor settings

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Settings Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISiteSettingsService siteSettingsService, ILogger<SettingsController> logger)
    {
        _siteSettingsService = siteSettingsService;
        _logger = logger;
    }

    #region Internal Kanban Settings

    /// <summary>
    /// Get internal kanban duplication settings
    /// </summary>
    /// <returns>Internal kanban settings</returns>
    /// <response code="200">Returns internal kanban settings</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("internal-kanban")]
    [ProducesResponseType(typeof(ApiResponse<InternalKanbanSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInternalKanbanSettings()
    {
        _logger.LogInformation("Getting internal kanban settings");

        var siteSettingsResponse = await _siteSettingsService.GetSiteSettingsAsync();

        if (!siteSettingsResponse.Success || siteSettingsResponse.Data == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, siteSettingsResponse);
        }

        // Map SiteSettings to InternalKanbanSettingsDto
        var kanbanSettings = new InternalKanbanSettingsDto
        {
            SettingId = siteSettingsResponse.Data.SettingId,
            AllowDuplicates = siteSettingsResponse.Data.KanbanAllowDuplicates,
            DuplicateWindowHours = siteSettingsResponse.Data.KanbanDuplicateWindowHours,
            AlertOnDuplicate = siteSettingsResponse.Data.KanbanAlertOnDuplicate,
            ModifiedAt = siteSettingsResponse.Data.ModifiedAt
        };

        var response = ApiResponse<InternalKanbanSettingsDto>.SuccessResponse(
            kanbanSettings,
            "Internal kanban settings retrieved successfully"
        );

        return Ok(response);
    }

    /// <summary>
    /// Save internal kanban duplication settings (Admin only)
    /// </summary>
    /// <param name="request">Internal kanban settings to save</param>
    /// <returns>Saved internal kanban settings</returns>
    /// <response code="200">Settings saved successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("internal-kanban")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<InternalKanbanSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveInternalKanbanSettings([FromBody] UpdateInternalKanbanSettingsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Saving internal kanban settings");

        // First, get current settings
        var currentSettingsResponse = await _siteSettingsService.GetSiteSettingsAsync();
        if (!currentSettingsResponse.Success || currentSettingsResponse.Data == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, currentSettingsResponse);
        }

        // Create update request with kanban settings changed
        var updateRequest = new UpdateSiteSettingsRequest
        {
            // Site settings - keep existing values
            PlantLocation = currentSettingsResponse.Data.PlantLocation,
            PlantOpeningTime = currentSettingsResponse.Data.PlantOpeningTime,
            PlantClosingTime = currentSettingsResponse.Data.PlantClosingTime,
            EnablePreShipmentScan = currentSettingsResponse.Data.EnablePreShipmentScan,

            // Dock settings - keep existing values
            DockBehindThreshold = currentSettingsResponse.Data.DockBehindThreshold,
            DockCriticalThreshold = currentSettingsResponse.Data.DockCriticalThreshold,
            DockDisplayMode = currentSettingsResponse.Data.DockDisplayMode,
            DockRefreshInterval = currentSettingsResponse.Data.DockRefreshInterval,
            DockOrderLookbackHours = currentSettingsResponse.Data.DockOrderLookbackHours,

            // Kanban settings - update with new values
            KanbanAllowDuplicates = request.AllowDuplicates,
            KanbanDuplicateWindowHours = request.DuplicateWindowHours,
            KanbanAlertOnDuplicate = request.AlertOnDuplicate
        };

        var userId = GetCurrentUserId();
        var updateResponse = await _siteSettingsService.UpdateSiteSettingsAsync(updateRequest, userId);

        if (!updateResponse.Success || updateResponse.Data == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, updateResponse);
        }

        // Map response back to InternalKanbanSettingsDto
        var kanbanSettings = new InternalKanbanSettingsDto
        {
            SettingId = updateResponse.Data.SettingId,
            AllowDuplicates = updateResponse.Data.KanbanAllowDuplicates,
            DuplicateWindowHours = updateResponse.Data.KanbanDuplicateWindowHours,
            AlertOnDuplicate = updateResponse.Data.KanbanAlertOnDuplicate,
            ModifiedAt = updateResponse.Data.ModifiedAt
        };

        var response = ApiResponse<InternalKanbanSettingsDto>.SuccessResponse(
            kanbanSettings,
            "Internal kanban settings saved successfully"
        );

        return Ok(response);
    }

    #endregion

    #region Dock Monitor Settings

    /// <summary>
    /// Get global dock monitor display settings (system-wide)
    /// </summary>
    /// <returns>Dock monitor settings</returns>
    /// <response code="200">Returns dock monitor settings</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("dock-monitor")]
    [ProducesResponseType(typeof(ApiResponse<DockMonitorSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDockMonitorSettings()
    {
        _logger.LogInformation("Getting global dock monitor settings");

        var siteSettingsResponse = await _siteSettingsService.GetSiteSettingsAsync();

        if (!siteSettingsResponse.Success || siteSettingsResponse.Data == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, siteSettingsResponse);
        }

        // Map SiteSettings to DockMonitorSettingsDto
        var dockSettings = new DockMonitorSettingsDto
        {
            SettingId = siteSettingsResponse.Data.SettingId,
            UserId = null, // Global settings have no specific user
            BehindThreshold = siteSettingsResponse.Data.DockBehindThreshold,
            CriticalThreshold = siteSettingsResponse.Data.DockCriticalThreshold,
            DisplayMode = siteSettingsResponse.Data.DockDisplayMode,
            SelectedLocations = new List<string>(), // This is now stored in PlantLocation
            RefreshInterval = siteSettingsResponse.Data.DockRefreshInterval,
            ModifiedAt = siteSettingsResponse.Data.ModifiedAt
        };

        var response = ApiResponse<DockMonitorSettingsDto>.SuccessResponse(
            dockSettings,
            "Dock monitor settings retrieved successfully"
        );

        return Ok(response);
    }

    /// <summary>
    /// Save global dock monitor display settings (system-wide) - Admin only
    /// </summary>
    /// <param name="request">Dock monitor settings to save</param>
    /// <returns>Saved dock monitor settings</returns>
    /// <response code="200">Settings saved successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("dock-monitor")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DockMonitorSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveDockMonitorSettings([FromBody] UpdateDockMonitorSettingsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Saving global dock monitor settings");

        // First, get current settings
        var currentSettingsResponse = await _siteSettingsService.GetSiteSettingsAsync();
        if (!currentSettingsResponse.Success || currentSettingsResponse.Data == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, currentSettingsResponse);
        }

        // Create update request with dock settings changed
        var updateRequest = new UpdateSiteSettingsRequest
        {
            // Site settings - keep existing values
            PlantLocation = currentSettingsResponse.Data.PlantLocation,
            PlantOpeningTime = currentSettingsResponse.Data.PlantOpeningTime,
            PlantClosingTime = currentSettingsResponse.Data.PlantClosingTime,
            EnablePreShipmentScan = currentSettingsResponse.Data.EnablePreShipmentScan,

            // Dock settings - update with new values
            DockBehindThreshold = request.BehindThreshold,
            DockCriticalThreshold = request.CriticalThreshold,
            DockDisplayMode = request.DisplayMode,
            DockRefreshInterval = currentSettingsResponse.Data.DockRefreshInterval, // Not in request
            DockOrderLookbackHours = currentSettingsResponse.Data.DockOrderLookbackHours, // Not in request

            // Kanban settings - keep existing values
            KanbanAllowDuplicates = currentSettingsResponse.Data.KanbanAllowDuplicates,
            KanbanDuplicateWindowHours = currentSettingsResponse.Data.KanbanDuplicateWindowHours,
            KanbanAlertOnDuplicate = currentSettingsResponse.Data.KanbanAlertOnDuplicate
        };

        var userId = GetCurrentUserId();
        var updateResponse = await _siteSettingsService.UpdateSiteSettingsAsync(updateRequest, userId);

        if (!updateResponse.Success || updateResponse.Data == null)
        {
            if (updateResponse.Message.Contains("Invalid thresholds"))
            {
                return BadRequest(updateResponse);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, updateResponse);
        }

        // Map response back to DockMonitorSettingsDto
        var dockSettings = new DockMonitorSettingsDto
        {
            SettingId = updateResponse.Data.SettingId,
            UserId = null, // Global settings have no specific user
            BehindThreshold = updateResponse.Data.DockBehindThreshold,
            CriticalThreshold = updateResponse.Data.DockCriticalThreshold,
            DisplayMode = updateResponse.Data.DockDisplayMode,
            SelectedLocations = request.SelectedLocations,
            RefreshInterval = updateResponse.Data.DockRefreshInterval,
            ModifiedAt = updateResponse.Data.ModifiedAt
        };

        var response = ApiResponse<DockMonitorSettingsDto>.SuccessResponse(
            dockSettings,
            "Dock monitor settings saved successfully"
        );

        return Ok(response);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get current user ID from JWT token claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    #endregion
}
