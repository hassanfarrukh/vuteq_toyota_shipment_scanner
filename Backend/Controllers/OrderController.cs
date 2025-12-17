// Author: Hassan
// Date: 2025-12-17
// Description: Controller for Order operations - handles API endpoints for order queries

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for Order operations
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get built skids for an order from tblSkidScans
    /// </summary>
    /// <remarks>
    /// Retrieve all distinct skids that have been built for a specific order.
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/orders/2025121134/skids?dockCode=FB
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Found 2 skid(s) for order 2025121134",
    ///   "data": {
    ///     "orderNumber": "2025121134",
    ///     "dockCode": "FB",
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "skids": [
    ///       {
    ///         "skidId": "001A",
    ///         "skidNumber": "001",
    ///         "skidSide": "A",
    ///         "palletizationCode": "LB",
    ///         "scannedAt": "2025-12-17T10:30:00Z"
    ///       },
    ///       {
    ///         "skidId": "002A",
    ///         "skidNumber": "002",
    ///         "skidSide": "A",
    ///         "palletizationCode": "LB",
    ///         "scannedAt": "2025-12-17T10:35:00Z"
    ///       }
    ///     ],
    ///     "totalSkids": 2
    ///   }
    /// }
    /// ```
    ///
    /// **No Skids Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "No skids found for order 2025121134",
    ///   "data": {
    ///     "orderNumber": "2025121134",
    ///     "dockCode": "FB",
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "skids": [],
    ///     "totalSkids": 0
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="orderNumber">Order number (e.g., "2025121134")</param>
    /// <param name="dockCode">Dock code (e.g., "FB")</param>
    /// <returns>List of skids built for this order</returns>
    /// <response code="200">Skids retrieved successfully (may be empty array)</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{orderNumber}/skids")]
    [ProducesResponseType(typeof(ApiResponse<OrderSkidsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<OrderSkidsResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderSkids(
        [FromRoute] string orderNumber,
        [FromQuery] string dockCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(dockCode))
            {
                return BadRequest(ApiResponse<OrderSkidsResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Order number and dock code are required"));
            }

            var result = await _orderService.GetOrderSkidsAsync(orderNumber, dockCode);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving skids for order: {OrderNumber}-{DockCode}",
                orderNumber, dockCode);
            return StatusCode(500, ApiResponse<OrderSkidsResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving order skids"));
        }
    }
}
