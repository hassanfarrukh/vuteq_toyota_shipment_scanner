// Author: Hassan
// Date: 2025-01-03
// Description: Controller for Site Settings management - provides endpoints for consolidated site-wide settings

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Site Settings Management API endpoints
/// Consolidated settings from Site, Dock Monitor, and Internal Kanban tabs
/// </summary>
[ApiController]
[Route("api/v1/site-settings")]
[Authorize]
[Produces("application/json")]
public class SiteSettingsController : ControllerBase
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly ILogger<SiteSettingsController> _logger;

    public SiteSettingsController(
        ISiteSettingsService siteSettingsService,
        ILogger<SiteSettingsController> logger)
    {
        _siteSettingsService = siteSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get consolidated site settings
    /// </summary>
    /// <returns>Site settings including Site, Dock Monitor, and Internal Kanban configurations</returns>
    /// <response code="200">Returns site settings</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSiteSettings()
    {
        _logger.LogInformation("Getting site settings");

        var response = await _siteSettingsService.GetSiteSettingsAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Update consolidated site settings (Admin only)
    /// </summary>
    /// <param name="request">Site settings to update</param>
    /// <returns>Updated site settings</returns>
    /// <response code="200">Settings updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSiteSettings([FromBody] UpdateSiteSettingsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating site settings by user {UserId}", userId);

        var response = await _siteSettingsService.UpdateSiteSettingsAsync(request, userId);

        if (!response.Success)
        {
            if (response.Message.Contains("Invalid thresholds") ||
                response.Message.Contains("Invalid plant hours"))
            {
                return BadRequest(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

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
