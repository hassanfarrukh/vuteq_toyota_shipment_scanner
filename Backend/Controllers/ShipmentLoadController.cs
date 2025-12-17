// Author: Hassan
// Date: 2025-12-17
// Description: Controller for Shipment Load operations - Toyota SCS integration with session management

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for Shipment Load operations - Toyota SCS Trailer API integration
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

    // ===== SESSION MANAGEMENT ENDPOINTS =====

    /// <summary>
    /// Start a new session or resume existing session for a route
    /// </summary>
    /// <remarks>
    /// Creates a new shipment load session or resumes an existing active session for the specified route.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/session/start
    /// {
    ///   "routeNumber": "YUAN03",
    ///   "supplierCode": "56408",
    ///   "pickupDateTime": "2024-02-12T14:11:00",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Session start request</param>
    /// <returns>Session details with orders and exceptions</returns>
    [HttpPost("session/start")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Start session request - Route: {RouteNumber}", request?.RouteNumber);

            if (request == null)
            {
                return BadRequest(ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.StartOrResumeSessionAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Session started/resumed - SessionId: {SessionId}, IsResumed: {IsResumed}",
                    result.Data?.SessionId, result.Data?.IsResumed);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error starting session for route: {RouteNumber}", request?.RouteNumber);
            return StatusCode(500, ApiResponse<SessionResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Update session with trailer information
    /// </summary>
    /// <remarks>
    /// Updates trailer, seal, driver, and supplier information for an active session.
    ///
    /// **Sample Request:**
    /// ```json
    /// PUT /api/v1/shipment-load/session/{sessionId}
    /// {
    ///   "trailerNumber": "614144",
    ///   "sealNumber": "000210002",
    ///   "lpCode": "RYDD",
    ///   "driverFirstName": "Brian",
    ///   "driverLastName": "OConnor",
    ///   "supplierFirstName": "Dominic",
    ///   "supplierLastName": "Toretto"
    /// }
    /// ```
    /// </remarks>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Update request with trailer information</param>
    /// <returns>Updated session details</returns>
    [HttpPut("session/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSession([FromRoute] Guid sessionId, [FromBody] UpdateSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Update session request - SessionId: {SessionId}", sessionId);

            if (request == null)
            {
                return BadRequest(ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SessionResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.UpdateSessionAsync(sessionId, request);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Session updated - SessionId: {SessionId}", sessionId);
                return Ok(result);
            }

            return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error updating session: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<SessionResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get session details with orders and exceptions
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Session details</returns>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSession([FromRoute] Guid sessionId)
    {
        try
        {
            var result = await _shipmentLoadService.GetSessionAsync(sessionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error retrieving session: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<SessionResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    // ===== SCAN OPERATIONS =====

    /// <summary>
    /// Scan and validate an order for shipment loading
    /// </summary>
    /// <remarks>
    /// Validates an order and links it to the active session.
    ///
    /// **Validation Rules:**
    /// 1. Session exists and is active
    /// 2. Order exists with matching OrderNumber + DockCode
    /// 3. Order.Status >= SkidBuilt (was built)
    /// 4. Order.PlannedRoute matches session route
    /// 5. Order.Status != Shipped (not already shipped)
    /// 6. SkidScans exist for order (proves skid was built)
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/scan
    /// {
    ///   "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "orderNumber": "2023080205",
    ///   "dockCode": "V8",
    ///   "palletizationCode": "LB",
    ///   "mros": "05",
    ///   "skidId": "001A"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Scan request</param>
    /// <returns>Validation result</returns>
    [HttpPost("scan")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadScanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanOrder([FromBody] ShipmentLoadScanRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Scan request - SessionId: {SessionId}, OrderNumber: {OrderNumber}",
                request?.SessionId, request?.OrderNumber);

            if (request == null)
            {
                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.ValidateAndScanOrderAsync(request);

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
            _logger.LogError(ex, "[SHIPMENT LOAD] Error scanning order: {OrderNumber}", request?.OrderNumber);
            return StatusCode(500, ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    // ===== EXCEPTION OPERATIONS =====

    /// <summary>
    /// Add exception to session (trailer-level or skid-level)
    /// </summary>
    /// <remarks>
    /// Adds an exception code to the shipment session. Can be trailer-level (no RelatedSkidId) or skid-level (with RelatedSkidId).
    ///
    /// **Trailer-level Exception Codes:** 13, 17, 24, 99
    /// **Skid-level Exception Codes:** 14, 15, 18, 19, 21, 22
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/exception
    /// {
    ///   "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "exceptionType": "13",
    ///   "comments": "Blowout - Space/Weight issue",
    ///   "relatedSkidId": null,
    ///   "createdByUserId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Exception add request</param>
    /// <returns>Created exception details</returns>
    [HttpPost("exception")]
    [ProducesResponseType(typeof(ApiResponse<ExceptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddException([FromBody] AddExceptionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Add exception - SessionId: {SessionId}, Type: {ExceptionType}",
                request?.SessionId, request?.ExceptionType);

            if (request == null)
            {
                return BadRequest(ApiResponse<ExceptionDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ExceptionDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.AddExceptionAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error adding exception to session: {SessionId}", request?.SessionId);
            return StatusCode(500, ApiResponse<ExceptionDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Remove exception from session
    /// </summary>
    /// <param name="exceptionId">Exception ID to remove</param>
    /// <returns>Success result</returns>
    [HttpDelete("exception/{exceptionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveException([FromRoute] Guid exceptionId)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Remove exception - ExceptionId: {ExceptionId}", exceptionId);

            var result = await _shipmentLoadService.RemoveExceptionAsync(exceptionId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error removing exception: {ExceptionId}", exceptionId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    // ===== COMPLETION OPERATIONS =====

    /// <summary>
    /// Complete shipment and submit to Toyota API
    /// </summary>
    /// <remarks>
    /// Completes the shipment session by:
    /// 1. Building Toyota API payload from session, orders, and skid scans
    /// 2. Submitting to Toyota /trailer endpoint
    /// 3. Updating all orders to Shipped status
    /// 4. Storing Toyota confirmation number
    ///
    /// **IMPORTANT:** Driver name is REQUIRED (dropHook=false for VUTEQ).
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/shipment-load/complete
    /// {
    ///   "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Completion request</param>
    /// <returns>Toyota confirmation and shipment details</returns>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadCompleteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteShipment([FromBody] ShipmentLoadCompleteRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Complete request - SessionId: {SessionId}", request?.SessionId);

            if (request == null)
            {
                return BadRequest(ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _shipmentLoadService.CompleteShipmentAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Shipment completed - SessionId: {SessionId}, Toyota Confirmation: {ConfirmationNumber}",
                    request.SessionId, result.Data?.ConfirmationNumber);
                return Ok(result);
            }

            _logger.LogWarning("[SHIPMENT LOAD] Complete shipment failed: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error completing shipment - SessionId: {SessionId}", request?.SessionId);
            return StatusCode(500, ApiResponse<ShipmentLoadCompleteResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    // ===== VALIDATION OPERATIONS (without session) =====

    /// <summary>
    /// Validate order and get skid count WITHOUT starting a session
    /// </summary>
    /// <remarks>
    /// Validates that an order exists, checks if skid-build is complete, and returns the skid count.
    /// Does NOT create a session.
    ///
    /// **Validation Logic:**
    /// 1. Find order by OrderNumber + DockCode
    /// 2. Check if order exists → return error if not
    /// 3. Check if Status >= SkidBuilt → return skidBuildComplete = true/false
    /// 4. Count skids from tblSkidScans for this order
    /// 5. Return order info + skid count
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/shipment-load/validate-order?orderNumber=2025121134&amp;dockCode=FB
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Order 2025121134 validated successfully. Skid-build complete: true, Skid count: 3",
    ///   "data": {
    ///     "success": true,
    ///     "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "orderNumber": "2025121134",
    ///     "dockCode": "FB",
    ///     "plantCode": "02TMI",
    ///     "supplierCode": "56408",
    ///     "status": "SkidBuilt",
    ///     "skidBuildComplete": true,
    ///     "skidCount": 3,
    ///     "toyotaConfirmationNumber": "TYT-2024-001234"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="orderNumber">Order number (e.g., "2025121134")</param>
    /// <param name="dockCode">Dock code (e.g., "FB")</param>
    /// <returns>Order validation result with skid count</returns>
    [HttpGet("validate-order")]
    [ProducesResponseType(typeof(ApiResponse<ValidateOrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateOrder(
        [FromQuery] string orderNumber,
        [FromQuery] string dockCode)
    {
        try
        {
            _logger.LogInformation("[SHIPMENT LOAD] Validate order request - OrderNumber: {OrderNumber}, DockCode: {DockCode}",
                orderNumber, dockCode);

            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return BadRequest(ApiResponse<ValidateOrderResponseDto>.ErrorResponse(
                    "Invalid request", "OrderNumber is required"));
            }

            if (string.IsNullOrWhiteSpace(dockCode))
            {
                return BadRequest(ApiResponse<ValidateOrderResponseDto>.ErrorResponse(
                    "Invalid request", "DockCode is required"));
            }

            var result = await _shipmentLoadService.ValidateOrderAsync(orderNumber, dockCode);

            if (result.Success)
            {
                _logger.LogInformation("[SHIPMENT LOAD] Order validated - OrderNumber: {OrderNumber}, SkidCount: {SkidCount}",
                    orderNumber, result.Data?.SkidCount);
                return Ok(result);
            }

            _logger.LogWarning("[SHIPMENT LOAD] Order validation failed: {Message}", result.Message);
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SHIPMENT LOAD] Error validating order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            return StatusCode(500, ApiResponse<ValidateOrderResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    // ===== LEGACY ENDPOINTS (backwards compatibility) =====

    /// <summary>
    /// Get all orders for a route that are ready to ship (legacy endpoint)
    /// </summary>
    /// <param name="routeNumber">Route number</param>
    /// <returns>List of orders ready to ship</returns>
    [HttpGet("route/{routeNumber}")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadRouteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersByRoute([FromRoute] string routeNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(routeNumber))
            {
                return BadRequest(ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                    "Invalid request", "Route number is required"));
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
            _logger.LogError(ex, "Error retrieving orders for route: {RouteNumber}", routeNumber);
            return StatusCode(500, ApiResponse<ShipmentLoadRouteResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }
}
