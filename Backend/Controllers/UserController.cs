// Author: Hassan
// Date: 2025-11-25
// Updated: 2026-01-16 - Added audit field assignments (Hassan)
// Description: Controller for User management - provides CRUD endpoints + Get All Users (active/inactive)

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// User Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active users
    /// </summary>
    /// <returns>List of active users</returns>
    /// <response code="200">Returns list of users</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Getting all active users");

        var response = await _userService.GetAllUsersAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get all users (active and inactive)
    /// </summary>
    /// <returns>List of all users including inactive ones</returns>
    /// <response code="200">Returns list of all users</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsersIncludingInactive()
    {
        _logger.LogInformation("Getting all users (active and inactive)");

        var response = await _userService.GetAllUsersIncludingInactiveAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns user details</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", id);

        var response = await _userService.GetUserByIdAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user details</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="409">Conflict - user ID already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        _logger.LogInformation("Creating new user: {Name} by user {UserId}", request.Name, currentUserId);

        var response = await _userService.CreateUserAsync(request, currentUserId);

        if (!response.Success)
        {
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = response.Data?.UserId },
            response
        );
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user details</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        _logger.LogInformation("Updating user: {UserId} by user {CurrentUserId}", id, currentUserId);

        var response = await _userService.UpdateUserAsync(id, request, currentUserId);

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
    /// Delete a user (soft delete)
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);

        var response = await _userService.DeleteUserAsync(id);

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
