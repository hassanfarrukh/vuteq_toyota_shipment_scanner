// Author: Hassan
// Date: 2025-01-13
// Description: Controller for Internal Kanban Exclusion management - provides CRUD and bulk upload endpoints

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Internal Kanban Exclusion Management API endpoints
/// </summary>
[ApiController]
[Route("api/internal-kanban-exclusions")]
[Authorize]
[Produces("application/json")]
public class InternalKanbanExclusionsController : ControllerBase
{
    private readonly IInternalKanbanExclusionService _service;
    private readonly ILogger<InternalKanbanExclusionsController> _logger;

    public InternalKanbanExclusionsController(
        IInternalKanbanExclusionService service,
        ILogger<InternalKanbanExclusionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all internal kanban exclusions
    /// </summary>
    /// <returns>List of all internal kanban exclusions</returns>
    /// <response code="200">Returns list of exclusions</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InternalKanbanExclusionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all internal kanban exclusions");

        var response = await _service.GetAllAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get internal kanban exclusion by ID
    /// </summary>
    /// <param name="id">Exclusion ID (GUID)</param>
    /// <returns>Exclusion details</returns>
    /// <response code="200">Returns exclusion details</response>
    /// <response code="404">Exclusion not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InternalKanbanExclusionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting internal kanban exclusion by ID: {ExclusionId}", id);

        var response = await _service.GetByIdAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create a new internal kanban exclusion (single mode)
    /// </summary>
    /// <param name="request">Exclusion creation request (PartNumber and InternalKanbanExclusion only)</param>
    /// <returns>Created exclusion details</returns>
    /// <response code="201">Exclusion created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="409">Conflict - part number already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InternalKanbanExclusionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInternalKanbanExclusionDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                "Invalid user token",
                "Unable to identify user from token"
            ));
        }

        _logger.LogInformation("Creating internal kanban exclusion: {PartNumber} by user {UserId}",
            request.PartNumber, userId);

        var response = await _service.CreateAsync(request, userId);

        if (!response.Success)
        {
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Data?.ExclusionId },
            response
        );
    }

    /// <summary>
    /// Bulk upload internal kanban exclusions from Excel file (bulk mode)
    /// </summary>
    /// <param name="file">Excel file with single sheet containing 'PartNumber' column</param>
    /// <returns>Bulk upload results with success/failed counts and error details</returns>
    /// <response code="200">Bulk upload completed (may have partial failures - check response for details)</response>
    /// <response code="400">Invalid file or file format</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPost("bulk-upload")]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkUpload(IFormFile file)
    {
        // Get current user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<BulkUploadResultDto>.ErrorResponse(
                "Invalid user token",
                "Unable to identify user from token"
            ));
        }

        _logger.LogInformation("Bulk upload internal kanban exclusions by user {UserId}", userId);

        var response = await _service.BulkUploadAsync(file, userId);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Update an existing internal kanban exclusion
    /// </summary>
    /// <param name="id">Exclusion ID (GUID)</param>
    /// <param name="request">Exclusion update request</param>
    /// <returns>Updated exclusion details</returns>
    /// <response code="200">Exclusion updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Exclusion not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="409">Conflict - part number already exists in another exclusion</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InternalKanbanExclusionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInternalKanbanExclusionDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                "Invalid user token",
                "Unable to identify user from token"
            ));
        }

        _logger.LogInformation("Updating internal kanban exclusion: {ExclusionId} by user {UserId}",
            id, userId);

        var response = await _service.UpdateAsync(id, request, userId);

        if (!response.Success)
        {
            if (response.Message.Contains("does not exist"))
            {
                return NotFound(response);
            }
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete an internal kanban exclusion
    /// </summary>
    /// <param name="id">Exclusion ID (GUID)</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Exclusion deleted successfully</response>
    /// <response code="404">Exclusion not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting internal kanban exclusion: {ExclusionId}", id);

        var response = await _service.DeleteAsync(id);

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
}
