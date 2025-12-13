// Author: Hassan
// Date: 2025-12-13
// Description: Controller for Toyota API Configuration management - Admin-only endpoints

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Toyota API Configuration Management API endpoints (Admin Only)
/// </summary>
[ApiController]
[Route("api/v1/toyota-config")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ToyotaConfigController : ControllerBase
{
    private readonly IToyotaConfigService _toyotaConfigService;
    private readonly ILogger<ToyotaConfigController> _logger;

    public ToyotaConfigController(
        IToyotaConfigService toyotaConfigService,
        ILogger<ToyotaConfigController> logger)
    {
        _toyotaConfigService = toyotaConfigService;
        _logger = logger;
    }

    /// <summary>
    /// Get all Toyota API configurations
    /// </summary>
    /// <returns>List of Toyota API configurations</returns>
    /// <response code="200">Returns list of Toyota API configurations</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ToyotaConfigResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllConfigs()
    {
        _logger.LogInformation("Getting all Toyota API configurations");

        var response = await _toyotaConfigService.GetAllConfigsAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get Toyota API configuration by ID
    /// </summary>
    /// <param name="configId">Configuration ID</param>
    /// <returns>Toyota API configuration details</returns>
    /// <response code="200">Returns Toyota API configuration</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{configId}")]
    [ProducesResponseType(typeof(ApiResponse<ToyotaConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConfigById(Guid configId)
    {
        _logger.LogInformation("Getting Toyota API configuration {ConfigId}", configId);

        var response = await _toyotaConfigService.GetConfigByIdAsync(configId);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get active Toyota API configuration by environment
    /// </summary>
    /// <param name="environment">Environment name (QA, PROD, DEV)</param>
    /// <returns>Active Toyota API configuration for the specified environment</returns>
    /// <response code="200">Returns active Toyota API configuration</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="404">No active configuration found for environment</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("environment/{environment}")]
    [ProducesResponseType(typeof(ApiResponse<ToyotaConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveConfigByEnvironment(string environment)
    {
        _logger.LogInformation("Getting active Toyota API configuration for environment {Environment}", environment);

        var response = await _toyotaConfigService.GetActiveConfigByEnvironmentAsync(environment);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create new Toyota API configuration
    /// </summary>
    /// <param name="request">Toyota API configuration details</param>
    /// <returns>Created Toyota API configuration</returns>
    /// <response code="201">Configuration created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ToyotaConfigResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateConfig([FromBody] ToyotaConfigCreateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating Toyota API configuration for environment {Environment} by user {UserId}",
            request.Environment, userId);

        var response = await _toyotaConfigService.CreateConfigAsync(request, userId.ToString());

        if (!response.Success)
        {
            if (response.Message.Contains("Invalid environment"))
            {
                return BadRequest(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return CreatedAtAction(
            nameof(GetConfigById),
            new { configId = response.Data?.ConfigId },
            response
        );
    }

    /// <summary>
    /// Update existing Toyota API configuration
    /// </summary>
    /// <param name="configId">Configuration ID to update</param>
    /// <param name="request">Updated configuration details</param>
    /// <returns>Updated Toyota API configuration</returns>
    /// <response code="200">Configuration updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{configId}")]
    [ProducesResponseType(typeof(ApiResponse<ToyotaConfigResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateConfig(Guid configId, [FromBody] ToyotaConfigUpdateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating Toyota API configuration {ConfigId} by user {UserId}",
            configId, userId);

        var response = await _toyotaConfigService.UpdateConfigAsync(configId, request, userId.ToString());

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }
            if (response.Message.Contains("Invalid environment"))
            {
                return BadRequest(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete Toyota API configuration
    /// </summary>
    /// <param name="configId">Configuration ID to delete</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Configuration deleted successfully</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{configId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteConfig(Guid configId)
    {
        _logger.LogInformation("Deleting Toyota API configuration {ConfigId}", configId);

        var response = await _toyotaConfigService.DeleteConfigAsync(configId);

        if (!response.Success)
        {
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Test Toyota API connection by attempting to obtain OAuth token
    /// </summary>
    /// <param name="configId">Configuration ID to test</param>
    /// <returns>Connection test result with token preview if successful</returns>
    /// <response code="200">Connection test completed (check response for success/failure)</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="403">Forbidden - admin role required</response>
    /// <response code="404">Configuration not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{configId}/test")]
    [ProducesResponseType(typeof(ApiResponse<ToyotaConnectionTestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestConnection(Guid configId)
    {
        _logger.LogInformation("Testing Toyota API connection for config {ConfigId}", configId);

        var response = await _toyotaConfigService.TestConnectionAsync(configId);

        // Note: We return 200 OK even if the connection test fails
        // The actual success/failure is indicated in the response.Data.Success property
        if (!response.Success && response.Message.Contains("not found"))
        {
            return NotFound(response);
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
