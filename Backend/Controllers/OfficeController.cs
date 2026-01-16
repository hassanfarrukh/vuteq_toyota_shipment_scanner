// Author: Hassan
// Date: 2025-11-24
// Updated: 2026-01-16 - Added audit field assignments (Hassan)
// Description: Controller for Office management - provides CRUD endpoints

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Office Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/admin/offices")]
[Authorize]
[Produces("application/json")]
public class OfficeController : ControllerBase
{
    private readonly IOfficeService _officeService;
    private readonly ILogger<OfficeController> _logger;

    public OfficeController(IOfficeService officeService, ILogger<OfficeController> logger)
    {
        _officeService = officeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all offices (active and inactive)
    /// </summary>
    /// <returns>List of all offices</returns>
    /// <response code="200">Returns list of offices</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OfficeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOffices()
    {
        _logger.LogInformation("Getting all offices (active and inactive)");

        var response = await _officeService.GetAllOfficesAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get office by ID
    /// </summary>
    /// <param name="id">Office ID (GUID)</param>
    /// <returns>Office details</returns>
    /// <response code="200">Returns office details</response>
    /// <response code="404">Office not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OfficeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOfficeById(Guid id)
    {
        _logger.LogInformation("Getting office by ID: {OfficeId}", id);

        var response = await _officeService.GetOfficeByIdAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create a new office
    /// </summary>
    /// <param name="request">Office creation request</param>
    /// <returns>Created office details</returns>
    /// <response code="201">Office created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="409">Conflict - office code already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OfficeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOffice([FromBody] CreateOfficeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating new office: {Code} by user {UserId}", request.Code, userId);

        var response = await _officeService.CreateOfficeAsync(request, userId);

        if (!response.Success)
        {
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return CreatedAtAction(
            nameof(GetOfficeById),
            new { id = response.Data?.OfficeId },
            response
        );
    }

    /// <summary>
    /// Update an existing office
    /// </summary>
    /// <param name="id">Office ID (GUID)</param>
    /// <param name="request">Office update request</param>
    /// <returns>Updated office details</returns>
    /// <response code="200">Office updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Office not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OfficeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateOffice(Guid id, [FromBody] UpdateOfficeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating office: {OfficeId} by user {UserId}", id, userId);

        var response = await _officeService.UpdateOfficeAsync(id, request, userId);

        if (!response.Success)
        {
            if (response.Message.Contains("does not exist"))
            {
                return NotFound(response);
            }
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete an office (soft delete)
    /// </summary>
    /// <param name="id">Office ID (GUID)</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Office deleted successfully</response>
    /// <response code="404">Office not found</response>
    /// <response code="400">Cannot delete office with active warehouses</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteOffice(Guid id)
    {
        _logger.LogInformation("Deleting office: {OfficeId}", id);

        var response = await _officeService.DeleteOfficeAsync(id);

        if (!response.Success)
        {
            if (response.Message.Contains("does not exist"))
            {
                return NotFound(response);
            }
            return BadRequest(response);
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
