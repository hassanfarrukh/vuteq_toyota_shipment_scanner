// Author: Hassan
// Date: 2025-12-24
// Description: Controller for Dock Monitor - provides real-time order data for dock monitor display

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Dock Monitor API endpoints for real-time order tracking
/// </summary>
[ApiController]
[Route("api/v1/dock-monitor")]
[Authorize]
[Produces("application/json")]
public class DockMonitorController : ControllerBase
{
    private readonly IDockMonitorService _dockMonitorService;
    private readonly ILogger<DockMonitorController> _logger;

    public DockMonitorController(
        IDockMonitorService dockMonitorService,
        ILogger<DockMonitorController> logger)
    {
        _dockMonitorService = dockMonitorService;
        _logger = logger;
    }

    /// <summary>
    /// Get dock monitor data with all shipments and orders
    /// </summary>
    /// <remarks>
    /// Returns real-time dock monitor data including:
    /// - All shipments from the last 1.5 days (36 hours)
    /// - Orders grouped by shipment/route
    /// - Order statuses calculated based on thresholds
    /// - Applied display mode and location filters from settings
    ///
    /// Order statuses:
    /// - COMPLETED: Both skid build and shipment load done
    /// - ON_TIME: Within threshold, on track
    /// - BEHIND: Past behind threshold but not critical
    /// - CRITICAL: Past critical threshold
    /// - PROJECT_SHORT: Has projected short exception
    /// - SHORT_SHIPPED: Has shortage exception
    ///
    /// Display modes (from settings):
    /// - FULL: Show all orders
    /// - SHIPMENT_ONLY: Show only shipment load completed orders
    /// - SKID_ONLY: Show only skid build completed orders
    /// - COMPLETION_ONLY: Show only fully completed orders
    /// </remarks>
    /// <returns>Dock monitor data with shipments and orders</returns>
    /// <response code="200">Returns dock monitor data successfully</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("data")]
    [ProducesResponseType(typeof(ApiResponse<DockMonitorResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDockMonitorData()
    {
        _logger.LogInformation("Getting dock monitor data");

        var response = await _dockMonitorService.GetDockMonitorDataAsync();

        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve dock monitor data: {Message}", response.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        _logger.LogInformation(
            "Dock monitor data retrieved: {ShipmentCount} shipments, {OrderCount} orders",
            response.Data?.Shipments.Count ?? 0,
            response.Data?.TotalOrders ?? 0);

        return Ok(response);
    }
}
