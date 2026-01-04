// Author: Hassan
// Date: 2025-12-06
// Description: Controller for Skid Build operations - handles API endpoints for skid building workflow

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for Skid Build operations
/// </summary>
[ApiController]
[Route("api/v1/skid-build")]
[Authorize]
public class SkidBuildController : ControllerBase
{
    private readonly ISkidBuildService _skidBuildService;
    private readonly ILogger<SkidBuildController> _logger;

    public SkidBuildController(
        ISkidBuildService skidBuildService,
        ILogger<SkidBuildController> logger)
    {
        _skidBuildService = skidBuildService;
        _logger = logger;
    }

    /// <summary>
    /// Get order by order number and dock code
    /// </summary>
    /// <remarks>
    /// Look up an order by RealOrderNumber and DockCode, returns order details with planned items.
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/skid-build/order/2023080205?dockCode=V8
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Order 2023080205 retrieved successfully with 5 planned items",
    ///   "data": {
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "orderNumber": "2023080205",
    ///     "dockCode": "V8",
    ///     "supplierCode": "02806",
    ///     "plantCode": "02TMI",
    ///     "status": "Planned",
    ///     "plannedItems": [
    ///       {
    ///         "plannedItemId": "110e8400-e29b-41d4-a716-446655440111",
    ///         "partNumber": "681010E250",
    ///         "kanbanNumber": "VH98",
    ///         "qpc": 45,
    ///         "totalBoxPlanned": 1,
    ///         "manifestNo": 12345678,
    ///         "palletizationCode": "LB",
    ///         "scannedCount": 0
    ///       }
    ///     ]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="orderNumber">Order number (e.g., "2023080205")</param>
    /// <param name="dockCode">Dock code (e.g., "V8")</param>
    /// <returns>Order details with planned items</returns>
    /// <response code="200">Order retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("order/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildOrderDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderByNumberAndDock(
        [FromRoute] string orderNumber,
        [FromQuery] string dockCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(dockCode))
            {
                return BadRequest(ApiResponse<SkidBuildOrderDto>.ErrorResponse(
                    "Invalid request",
                    "Order number and dock code are required"));
            }

            var result = await _skidBuildService.GetOrderByNumberAndDockAsync(orderNumber, dockCode);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving order: {OrderNumber}-{DockCode}",
                orderNumber, dockCode);
            return StatusCode(500, ApiResponse<SkidBuildOrderDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving order"));
        }
    }

    /// <summary>
    /// Get order by order number and dock code with items grouped by skid
    /// </summary>
    /// <remarks>
    /// Look up an order by RealOrderNumber and DockCode, returns order details with planned items grouped by ManifestNo.
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/skid-build/order/2023080205/grouped?dockCode=V8
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Order 2023080205 retrieved successfully with 2 skids and 5 total items",
    ///   "data": {
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "orderNumber": "2023080205",
    ///     "dockCode": "V8",
    ///     "supplierCode": "02806",
    ///     "plantCode": "02TMI",
    ///     "status": "Planned",
    ///     "skids": [
    ///       {
    ///         "skidId": "678A",
    ///         "manifestNo": 12345678,
    ///         "palletizationCode": "LB",
    ///         "plannedKanbans": [
    ///           {
    ///             "plannedItemId": "110e8400-e29b-41d4-a716-446655440111",
    ///             "partNumber": "681010E250",
    ///             "kanbanNumber": "VH98",
    ///             "qpc": 45,
    ///             "totalBoxPlanned": 1,
    ///             "manifestNo": 12345678,
    ///             "palletizationCode": "LB",
    ///             "scannedCount": 0
    ///           }
    ///         ]
    ///       },
    ///       {
    ///         "skidId": "679A",
    ///         "manifestNo": 12345679,
    ///         "palletizationCode": "LB",
    ///         "plannedKanbans": [
    ///           {
    ///             "plannedItemId": "110e8400-e29b-41d4-a716-446655440222",
    ///             "partNumber": "681010E251",
    ///             "kanbanNumber": "VH99",
    ///             "qpc": 30,
    ///             "totalBoxPlanned": 2,
    ///             "manifestNo": 12345679,
    ///             "palletizationCode": "LB",
    ///             "scannedCount": 0
    ///           }
    ///         ]
    ///       }
    ///     ]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="orderNumber">Order number (e.g., "2023080205")</param>
    /// <param name="dockCode">Dock code (e.g., "V8")</param>
    /// <returns>Order details with planned items grouped by skid</returns>
    /// <response code="200">Order retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("order/{orderNumber}/grouped")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildOrderGroupedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildOrderGroupedDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderByNumberAndDockGrouped(
        [FromRoute] string orderNumber,
        [FromQuery] string dockCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(dockCode))
            {
                return BadRequest(ApiResponse<SkidBuildOrderGroupedDto>.ErrorResponse(
                    "Invalid request",
                    "Order number and dock code are required"));
            }

            var result = await _skidBuildService.GetOrderByNumberAndDockGroupedAsync(orderNumber, dockCode);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving grouped order: {OrderNumber}-{DockCode}",
                orderNumber, dockCode);
            return StatusCode(500, ApiResponse<SkidBuildOrderGroupedDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving grouped order"));
        }
    }

    /// <summary>
    /// Start a new skid build session
    /// </summary>
    /// <remarks>
    /// Start a new skid build session for an order.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/skid-build/session/start
    /// {
    ///   "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "skidNumber": 1,
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Session started successfully for skid #1",
    ///   "data": {
    ///     "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "skidNumber": 1,
    ///     "status": "active",
    ///     "userId": "660e8400-e29b-41d4-a716-446655440666",
    ///     "createdAt": "2025-12-06T10:30:00Z"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Session start request</param>
    /// <returns>Created session details</returns>
    /// <response code="200">Session started successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("session/start")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildSessionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartSession([FromBody] SkidBuildStartSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[SKID BUILD] Start session request received - OrderId: {OrderId}, SkidNumber: {SkidNumber}",
                request?.OrderId, request?.SkidNumber);

            if (request == null)
            {
                _logger.LogWarning("[SKID BUILD] Request body is null");
                return BadRequest(ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                    "Invalid request",
                    "Request body is required"));
            }

            // Remove ModelState errors for optional UserId (frontend may send invalid GUID strings)
            ModelState.Remove(nameof(request.UserId));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("[SKID BUILD] Model validation failed: {Errors}", string.Join(", ", errors));

                return BadRequest(ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                    "Invalid request",
                    string.Join(", ", errors)));
            }

            var result = await _skidBuildService.StartSessionAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[SKID BUILD] Session started successfully - SessionId: {SessionId}",
                    result.Data?.SessionId);
                return Ok(result);
            }

            _logger.LogWarning("[SKID BUILD] Failed to start session: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SKID BUILD] Unexpected error starting session for Order: {OrderId}", request?.OrderId);
            return StatusCode(500, ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while starting session"));
        }
    }

    /// <summary>
    /// Record a scan (Toyota Kanban + Internal Kanban pair)
    /// </summary>
    /// <remarks>
    /// Record a scan during skid build workflow.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/skid-build/scan
    /// {
    ///   "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///   "plannedItemId": "110e8400-e29b-41d4-a716-446655440111",
    ///   "skidNumber": 1,
    ///   "boxNumber": 1,
    ///   "lineSideAddress": "SA-FDG",
    ///   "internalKanban": "MPE",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Scan recorded successfully for skid #1, box #1",
    ///   "data": {
    ///     "scanId": "880e8400-e29b-41d4-a716-446655440888",
    ///     "plannedItemId": "110e8400-e29b-41d4-a716-446655440111",
    ///     "skidNumber": 1,
    ///     "boxNumber": 1,
    ///     "lineSideAddress": "SA-FDG",
    ///     "internalKanban": "MPE",
    ///     "scannedAt": "2025-12-06T10:35:00Z",
    ///     "scannedBy": "660e8400-e29b-41d4-a716-446655440666"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Scan request</param>
    /// <returns>Scan record details</returns>
    /// <response code="200">Scan recorded successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("scan")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildScanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildScanResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecordScan([FromBody] SkidBuildScanRequestDto request)
    {
        try
        {
            // Remove ModelState errors for optional UserId (frontend may send invalid GUID strings)
            ModelState.Remove(nameof(request.UserId));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Please check your input"));
            }

            var result = await _skidBuildService.RecordScanAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error recording scan for Session: {SessionId}", request.SessionId);
            return StatusCode(500, ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while recording scan"));
        }
    }

    /// <summary>
    /// Record an exception
    /// </summary>
    /// <remarks>
    /// Record an exception during skid build workflow.
    /// Exception codes: "10" (Revised Quantity), "11" (Modified QPC), "12" (Short Shipment), "20" (Non-Standard Packaging)
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/skid-build/exception
    /// {
    ///   "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///   "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "exceptionCode": "10",
    ///   "comments": "Toyota quantity reduction",
    ///   "skidNumber": 1,
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Exception '10' recorded successfully",
    ///   "data": {
    ///     "exceptionId": "990e8400-e29b-41d4-a716-446655440999",
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "exceptionCode": "10",
    ///     "comments": "Toyota quantity reduction",
    ///     "skidNumber": 1,
    ///     "createdAt": "2025-12-06T10:40:00Z"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Exception request</param>
    /// <returns>Exception record details</returns>
    /// <response code="200">Exception recorded successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("exception")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildException>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildException>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecordException([FromBody] SkidBuildExceptionRequestDto request)
    {
        try
        {
            // Remove ModelState errors for optional UserId (frontend may send invalid GUID strings)
            ModelState.Remove(nameof(request.UserId));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<SkidBuildException>.ErrorResponse(
                    "Invalid request",
                    "Please check your input"));
            }

            var result = await _skidBuildService.RecordExceptionAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error recording exception for Order: {OrderId}", request.OrderId);
            return StatusCode(500, ApiResponse<SkidBuildException>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while recording exception"));
        }
    }

    /// <summary>
    /// Delete an exception by ID
    /// </summary>
    /// <remarks>
    /// Delete a skid build exception record.
    ///
    /// **Sample Request:**
    /// ```
    /// DELETE /api/v1/skid-build/exception/990e8400-e29b-41d4-a716-446655440999
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Exception 990e8400-e29b-41d4-a716-446655440999 deleted successfully",
    ///   "data": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="exceptionId">Exception ID</param>
    /// <returns>Delete confirmation</returns>
    /// <response code="200">Exception deleted successfully</response>
    /// <response code="404">Exception not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("exception/{exceptionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteException([FromRoute] Guid exceptionId)
    {
        try
        {
            if (exceptionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Invalid request",
                    "Exception ID is required"));
            }

            var result = await _skidBuildService.DeleteExceptionAsync(exceptionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting exception: {ExceptionId}", exceptionId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while deleting exception"));
        }
    }

    /// <summary>
    /// Complete and submit the skid build session
    /// </summary>
    /// <remarks>
    /// Complete and submit a skid build session.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/skid-build/session/complete
    /// {
    ///   "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Skid build completed successfully. Confirmation: SKB-1701234567890-1234",
    ///   "data": {
    ///     "confirmationNumber": "SKB-1701234567890-1234",
    ///     "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///     "totalScanned": 5,
    ///     "totalExceptions": 1,
    ///     "completedAt": "2025-12-06T11:00:00Z"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Session completion request containing sessionId and userId</param>
    /// <returns>Completion confirmation</returns>
    /// <response code="200">Session completed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("session/complete")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildCompleteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildCompleteResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteSession([FromBody] SkidBuildCompleteRequestDto request)
    {
        try
        {
            if (request.SessionId == Guid.Empty || request.UserId == Guid.Empty)
            {
                return BadRequest(ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Session ID and User ID are required"));
            }

            var result = await _skidBuildService.CompleteSessionAsync(request.SessionId, request.UserId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error completing session: {SessionId}", request.SessionId);
            return StatusCode(500, ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while completing session"));
        }
    }

    /// <summary>
    /// Get session details
    /// </summary>
    /// <remarks>
    /// Get session details with all scans and exceptions.
    ///
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/skid-build/session/770e8400-e29b-41d4-a716-446655440777
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Session retrieved successfully",
    ///   "data": {
    ///     "sessionId": "770e8400-e29b-41d4-a716-446655440777",
    ///     "orderId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "skidNumber": 1,
    ///     "status": "completed",
    ///     "userId": "660e8400-e29b-41d4-a716-446655440666",
    ///     "createdAt": "2025-12-06T10:30:00Z",
    ///     "completedAt": "2025-12-06T11:00:00Z",
    ///     "confirmationNumber": "SKB-1701234567890-1234"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Session details</returns>
    /// <response code="200">Session retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Session not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<SkidBuildSessionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSessionById([FromRoute] Guid sessionId)
    {
        try
        {
            if (sessionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                    "Invalid request",
                    "Session ID is required"));
            }

            var result = await _skidBuildService.GetSessionByIdAsync(sessionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving session: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving session"));
        }
    }

    /// <summary>
    /// Restart a skid build session - clears all scans and resets order
    /// </summary>
    /// <remarks>
    /// Restarts a skid build session by clearing all related data and resetting the order to Planned status.
    ///
    /// **IMPORTANT:** This endpoint will be BLOCKED if the order has already been confirmed by Toyota.
    ///
    /// **What happens during restart:**
    /// 1. Validates session exists
    /// 2. Gets the Order from session.OrderId
    /// 3. **BLOCKS if ToyotaSkidBuildStatus == "confirmed"** - returns error
    /// 4. Deletes all SkidScans for this order
    /// 5. Deletes all SkidBuildExceptions for this order
    /// 6. Resets Order to Planned status with Toyota fields cleared
    /// 7. Cancels the session
    ///
    /// **Sample Request:**
    /// ```
    /// POST /api/v1/skid-build/session/770e8400-e29b-41d4-a716-446655440777/restart
    /// ```
    ///
    /// **Sample Response (Success):**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Session restarted successfully. Order reset to Planned status.",
    ///   "data": {
    ///     "success": true,
    ///     "message": "Order 2023080205 has been reset. All scans and exceptions cleared.",
    ///     "newSessionId": null
    ///   }
    /// }
    /// ```
    ///
    /// **Sample Response (Blocked):**
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "Cannot restart - already confirmed by Toyota",
    ///   "errors": ["Order 2023080205 has been confirmed by Toyota (Confirmation: TYT-123). Restart is not allowed."]
    /// }
    /// ```
    /// </remarks>
    /// <param name="sessionId">Session ID to restart</param>
    /// <returns>Restart result</returns>
    /// <response code="200">Session restarted successfully</response>
    /// <response code="400">Cannot restart - order confirmed by Toyota or other validation error</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("session/{sessionId}/restart")]
    [ProducesResponseType(typeof(ApiResponse<RestartSessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RestartSessionResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RestartSessionResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestartSession([FromRoute] Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[SKID BUILD] Restart session request - SessionId: {SessionId}", sessionId);

            if (sessionId == Guid.Empty)
            {
                return BadRequest(ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                    "Invalid request",
                    "Session ID is required"));
            }

            var result = await _skidBuildService.RestartSessionAsync(sessionId);

            if (result.Success)
            {
                _logger.LogInformation("[SKID BUILD] Session restarted successfully - SessionId: {SessionId}", sessionId);
                return Ok(result);
            }

            // Check if it's a "not found" error
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }

            // Otherwise it's a validation error (e.g., already confirmed by Toyota)
            _logger.LogWarning("[SKID BUILD] Restart blocked: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SKID BUILD] Unexpected error restarting session: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while restarting session"));
        }
    }
}

/// <summary>
/// Request DTO for completing a skid build session
/// </summary>
public class SkidBuildCompleteRequestDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// User ID completing the session
    /// </summary>
    public Guid UserId { get; set; }
}
