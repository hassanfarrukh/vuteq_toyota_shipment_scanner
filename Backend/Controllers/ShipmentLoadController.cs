// Author: Hassan
// Date: 2025-12-08
// Description: Controller for Shipment Load operations - handles API endpoints for shipment loading workflow

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for Shipment Load operations
/// </summary>
[ApiController]
[Route("api/v1/shipment-load")]
[Authorize]
public class ShipmentLoadController : ControllerBase
{
    private readonly IShipmentLoadService _shipmentLoadService;
    private readonly ILogger<ShipmentLoadController> _logger;

    public ShipmentLoadController(
        IShipmentLoadService shipmentLoadService,
        ILogger<ShipmentLoadController> logger)
    {
        _shipmentLoadService = shipmentLoadService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders for a route that are ready to ship
    /// </summary>
    /// <remarks>
    /// Retrieves all orders for the specified route where Status >= SkidBuilt (ready for shipment).
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/shipment-load/route/IDRE-06
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Found 3 orders for route IDRE-06",
    ///   "data": {
    ///     "routeNumber": "IDRE-06",
    ///     "orders": [
    ///       {
    ///         "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///         "orderNumber": "2023080205",
    ///         "dockCode": "V8",
    ///         "supplierCode": "02806",
    ///         "plantCode": "02TMI",
    ///         "plannedRoute": "IDRE-06",
    ///         "status": "SkidBuilt",
    ///         "totalSkids": 5,
    ///         "isScanned": false
    ///       }
    ///     ],
    ///     "totalOrders": 3
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="routeNumber">Route number (e.g., "IDRE-06")</param>
    /// <returns>List of orders ready to ship</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">No orders found for route</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("route/{routeNumber}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadRouteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadRouteResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersByRoute([FromRoute] string routeNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(routeNumber))
            {
                return BadRequest(ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Route number is required"));
            }

            var result = await _shipmentLoadService.GetOrdersByRouteAsync(routeNumber);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving orders for route: {RouteNumber}", routeNumber);
            return StatusCode(500, ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving orders"));
        }
    }

    /// <summary>
    /// Scan and validate an order for shipment loading
    /// </summary>
    /// <remarks>
    /// Validates an order and updates its status to ShipmentLoading if all checks pass.
    ///
    /// **Validation Rules:**
    /// 1. Order exists with matching OrderNumber + DockCode
    /// 2. Order.Status >= SkidBuilt (was built)
    /// 3. Order.PlannedRoute matches current route
    /// 4. Order.Status != Shipped (not already shipped)
    /// 5. SkidScans exist for order (proves skid was built)
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/scan
    /// {
    ///   "orderNumber": "2023080205",
    ///   "dockCode": "V8",
    ///   "routeNumber": "IDRE-06",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Order 2023080205 scanned successfully",
    ///   "data": {
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "orderNumber": "2023080205",
    ///     "dockCode": "V8",
    ///     "status": "ShipmentLoading",
    ///     "validationMessage": "Order 2023080205 validated successfully. 5 skid(s) confirmed.",
    ///     "scannedAt": "2025-12-08T10:30:00Z"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Scan request with order number, dock code, and route</param>
    /// <returns>Validation result</returns>
    /// <response code="200">Order scanned successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("scan")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadScanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadScanResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanOrder([FromBody] ShipmentLoadScanRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Scan request received - OrderNumber: {OrderNumber}, DockCode: {DockCode}, Route: {RouteNumber}",
                request?.OrderNumber, request?.DockCode, request?.RouteNumber);

            if (request == null)
            {
                _logger.LogWarning("[SHIPMENT LOAD] Request body is null");
                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Request body is required"));
            }

            // Remove ModelState errors for optional UserId
            ModelState.Remove(nameof(request.UserId));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("[SHIPMENT LOAD] Model validation failed: {Errors}", string.Join(", ", errors));

                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request",
                    string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.ValidateAndScanSkidAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Order scanned successfully - OrderNumber: {OrderNumber}",
                    result.Data?.OrderNumber);
                return Ok(result);
            }

            _logger.LogWarning("[SHIPMENT LOAD] Scan validation failed: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Unexpected error scanning order: {OrderNumber}-{DockCode}",
                request?.OrderNumber, request?.DockCode);
            return StatusCode(500, ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while scanning order"));
        }
    }

    /// <summary>
    /// Complete shipment and mark all scanned orders as shipped
    /// </summary>
    /// <remarks>
    /// Completes the shipment for a route by updating all orders in ShipmentLoading status to Shipped.
    /// Updates ActualRoute, ActualPickupDate, Trailer, and generates a ShipmentConfirmation number.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/complete
    /// {
    ///   "routeNumber": "IDRE-06",
    ///   "trailerNumber": "TRL-12345",
    ///   "sealNumber": "SEAL-9876",
    ///   "driverName": "John Smith",
    ///   "carrierName": "ABC Transport",
    ///   "shipmentNotes": "All items loaded and secured",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Shipment completed successfully. 3 orders shipped. Confirmation: SL-1701234567890-1234",
    ///   "data": {
    ///     "confirmationNumber": "SL-1701234567890-1234",
    ///     "routeNumber": "IDRE-06",
    ///     "trailerNumber": "TRL-12345",
    ///     "totalOrdersShipped": 3,
    ///     "completedAt": "2025-12-08T11:00:00Z",
    ///     "shippedOrderNumbers": ["2023080205", "2023080206", "2023080207"]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Completion request with route, trailer, and shipment details</param>
    /// <returns>Completion confirmation with shipment details</returns>
    /// <response code="200">Shipment completed successfully</response>
    /// <response code="400">Invalid request or no orders to ship</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadCompleteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadCompleteResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteShipment([FromBody] ShipmentLoadCompleteRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Complete request received - Route: {RouteNumber}, Trailer: {TrailerNumber}",
                request?.RouteNumber, request?.TrailerNumber);

            if (request == null)
            {
                _logger.LogWarning("[SHIPMENT LOAD] Request body is null");
                return BadRequest(ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Request body is required"));
            }

            // Remove ModelState errors for optional UserId
            ModelState.Remove(nameof(request.UserId));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("[SHIPMENT LOAD] Model validation failed: {Errors}", string.Join(", ", errors));

                return BadRequest(ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Invalid request",
                    string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.CompleteShipmentAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Shipment completed successfully - Route: {RouteNumber}, Confirmation: {ConfirmationNumber}",
                    result.Data?.RouteNumber, result.Data?.ConfirmationNumber);
                return Ok(result);
            }

            _logger.LogWarning("[SHIPMENT LOAD] Complete shipment failed: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Unexpected error completing shipment for route: {RouteNumber}",
                request?.RouteNumber);
            return StatusCode(500, ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while completing shipment"));
        }
    }
}
