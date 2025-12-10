// Author: Hassan
// Date: 2025-11-24
// Description: Controller for Warehouse management - provides CRUD endpoints

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Warehouse Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/admin/warehouses")]
[Authorize]
[Produces("application/json")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all warehouses (active and inactive)
    /// </summary>
    /// <returns>List of all warehouses</returns>
    /// <response code="200">Returns list of warehouses</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WarehouseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllWarehouses()
    {
        _logger.LogInformation("Getting all warehouses (active and inactive)");

        var response = await _warehouseService.GetAllWarehousesAsync();

        if (!response.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    /// <param name="id">Warehouse ID (GUID)</param>
    /// <returns>Warehouse details</returns>
    /// <response code="200">Returns warehouse details</response>
    /// <response code="404">Warehouse not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseById(Guid id)
    {
        _logger.LogInformation("Getting warehouse by ID: {WarehouseId}", id);

        var response = await _warehouseService.GetWarehouseByIdAsync(id);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    /// <param name="request">Warehouse creation request</param>
    /// <returns>Created warehouse details</returns>
    /// <response code="201">Warehouse created successfully</response>
    /// <response code="400">Invalid request data, validation errors, or office not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="409">Conflict - warehouse code already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new warehouse: {Code}", request.Code);

        var response = await _warehouseService.CreateWarehouseAsync(request);

        if (!response.Success)
        {
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }
            return BadRequest(response);
        }

        return CreatedAtAction(
            nameof(GetWarehouseById),
            new { id = response.Data?.WarehouseId },
            response
        );
    }

    /// <summary>
    /// Update an existing warehouse
    /// </summary>
    /// <param name="id">Warehouse ID (GUID)</param>
    /// <param name="request">Warehouse update request</param>
    /// <returns>Updated warehouse details</returns>
    /// <response code="200">Warehouse updated successfully</response>
    /// <response code="400">Invalid request data, validation errors, or office not found</response>
    /// <response code="404">Warehouse not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating warehouse: {WarehouseId}", id);

        var response = await _warehouseService.UpdateWarehouseAsync(id, request);

        if (!response.Success)
        {
            if (response.Message.Contains("does not exist") && response.Message.Contains("Warehouse"))
            {
                return NotFound(response);
            }
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a warehouse (soft delete)
    /// </summary>
    /// <param name="id">Warehouse ID (GUID)</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Warehouse deleted successfully</response>
    /// <response code="404">Warehouse not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWarehouse(Guid id)
    {
        _logger.LogInformation("Deleting warehouse: {WarehouseId}", id);

        var response = await _warehouseService.DeleteWarehouseAsync(id);

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
