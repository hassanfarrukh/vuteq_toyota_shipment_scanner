// Author: Hassan
// Date: 2025-12-01
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
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
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

        var response = await _settingsService.GetInternalKanbanSettingsAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

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

        var response = await _settingsService.SaveInternalKanbanSettingsAsync(request);

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

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

        var response = await _settingsService.GetDockMonitorSettingsAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

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

        var response = await _settingsService.SaveDockMonitorSettingsAsync(request);

        if (!response.Success)
        {
            if (response.Message.Contains("Invalid thresholds"))
            {
                return BadRequest(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

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
