// Author: Hassan
// Date: 2025-12-31
// Description: Controller for Pre-Shipment operations - Manifest-based session creation before driver arrives

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for Pre-Shipment operations
/// Allows warehouse staff to prepare shipments before driver arrives
/// </summary>
[ApiController]
[Route("api/v1/pre-shipment")]
[Authorize]
public class PreShipmentController : ControllerBase
{
    private readonly IPreShipmentService _preShipmentService;
    private readonly IShipmentLoadService _shipmentLoadService;
    private readonly ILogger<PreShipmentController> _logger;

    public PreShipmentController(
        IPreShipmentService preShipmentService,
        IShipmentLoadService shipmentLoadService,
        ILogger<PreShipmentController> logger)
    {
        _preShipmentService = preShipmentService;
        _shipmentLoadService = shipmentLoadService;
        _logger = logger;
    }

    /// <summary>
    /// Create Pre-Shipment session from manifest scan
    /// </summary>
    /// <remarks>
    /// Creates a Pre-Shipment session by:
    /// 1. Parsing 44-byte manifest barcode
    /// 2. Extracting order number
    /// 3. Querying route number from order
    /// 4. Creating session for the entire route
    /// 5. Returning all planned orders and skids
    ///
    /// **Manifest Format (44 bytes):**
    /// - Plant Code (2)
    /// - Supplier Code (5)
    /// - Dock Code (2)
    /// - Order Number (10)
    /// - Load ID (2)
    /// - Palletization (2)
    /// - MROS (4)
    /// - Skid ID (4)
    /// - etc.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/pre-shipment/create-from-manifest
    /// {
    ///   "manifestBarcode": "0256408V82023080205LBLB05000001A0000000000000",
    ///   "scannedBy": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Create from manifest request</param>
    /// <returns>Pre-Shipment session with planned orders and skids</returns>
    [HttpPost("create-from-manifest")]
    [ProducesResponseType(typeof(ApiResponse<CreateFromManifestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFromManifest([FromBody] CreateFromManifestRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Create from manifest request - Manifest length: {Length}",
                request?.ManifestBarcode?.Length ?? 0);

            if (request == null)
            {
                return BadRequest(ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _preShipmentService.CreateFromManifestAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[PRE-SHIPMENT] Session created - SessionId: {SessionId}, Route: {Route}",
                    result.Data?.SessionId, result.Data?.RouteNumber);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error creating session from manifest");
            return StatusCode(500, ApiResponse<CreateFromManifestResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get list of all Pre-Shipment sessions
    /// </summary>
    /// <returns>List of Pre-Shipment sessions</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<List<PreShipmentListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList()
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Get list request");

            var result = await _preShipmentService.GetListAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error getting session list");
            return StatusCode(500, ApiResponse<List<PreShipmentListItemDto>>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get Pre-Shipment session details by ID
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Pre-Shipment session details</returns>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSession([FromRoute] Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Get session request - SessionId: {SessionId}", sessionId);

            var result = await _preShipmentService.GetSessionAsync(sessionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error getting session: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<SessionResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Scan skid in Pre-Shipment session
    /// </summary>
    /// <remarks>
    /// Validates and links skid to Pre-Shipment session.
    /// Can reuse ShipmentLoad scan endpoint since logic is the same.
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/pre-shipment/{sessionId}/scan-skid
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
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Scan request</param>
    /// <returns>Scan validation result</returns>
    [HttpPost("{sessionId}/scan-skid")]
    [ProducesResponseType(typeof(ApiResponse<ShipmentLoadScanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanSkid([FromRoute] Guid sessionId, [FromBody] ShipmentLoadScanRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Scan skid request - SessionId: {SessionId}, OrderNumber: {OrderNumber}",
                sessionId, request?.OrderNumber);

            if (request == null)
            {
                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            // Override session ID from route
            request.SessionId = sessionId;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            // Reuse ShipmentLoadService scan logic
            var result = await _shipmentLoadService.ValidateAndScanOrderAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[PRE-SHIPMENT] Skid scanned - OrderNumber: {OrderNumber}, SkidId: {SkidId}",
                    request.OrderNumber, request.SkidId);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error scanning skid - SessionId: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<ShipmentLoadScanResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Update Pre-Shipment session with trailer/driver information
    /// </summary>
    /// <remarks>
    /// Updates trailer number, seal number, driver info, and supplier info for Pre-Shipment session.
    ///
    /// **Sample Request:**
    /// ```json
    /// PUT /api/v1/pre-shipment/{sessionId}/trailer-info
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
    /// <param name="request">Update request</param>
    /// <returns>Updated session details</returns>
    [HttpPut("{sessionId}/trailer-info")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTrailerInfo([FromRoute] Guid sessionId, [FromBody] UpdateSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Update trailer info - SessionId: {SessionId}", sessionId);

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

            var result = await _preShipmentService.UpdateSessionAsync(sessionId, request);

            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message.Contains("not found") ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error updating trailer info - SessionId: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<SessionResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Complete Pre-Shipment session and submit to Toyota API
    /// </summary>
    /// <remarks>
    /// Completes Pre-Shipment session by:
    /// 1. Validating all required information is present
    /// 2. Building Toyota API payload
    /// 3. Submitting to Toyota /trailer endpoint
    /// 4. Updating all orders to Shipped status
    /// 5. Storing Toyota confirmation number
    ///
    /// **Sample Request:**
    /// ```json
    /// POST /api/v1/pre-shipment/{sessionId}/complete
    /// {
    ///   "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "userId": "660e8400-e29b-41d4-a716-446655440666"
    /// }
    /// ```
    /// </remarks>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Completion request</param>
    /// <returns>Toyota confirmation and shipment details</returns>
    [HttpPost("{sessionId}/complete")]
    [ProducesResponseType(typeof(ApiResponse<PreShipmentCompleteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Complete([FromRoute] Guid sessionId, [FromBody] PreShipmentCompleteRequestDto request)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Complete request - SessionId: {SessionId}", sessionId);

            if (request == null)
            {
                return BadRequest(ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                    "Invalid request", "Request body is required"));
            }

            // Override session ID from route
            request.SessionId = sessionId;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                    "Invalid request", string.Join(", ", errors)));
            }

            var result = await _preShipmentService.CompleteAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("[PRE-SHIPMENT] Completed - SessionId: {SessionId}, Toyota Confirmation: {ConfirmationNumber}",
                    sessionId, result.Data?.ConfirmationNumber);
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error completing session - SessionId: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<PreShipmentCompleteResponseDto>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Delete incomplete Pre-Shipment session
    /// </summary>
    /// <param name="sessionId">Session ID to delete</param>
    /// <returns>Success result</returns>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSession([FromRoute] Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[PRE-SHIPMENT] Delete request - SessionId: {SessionId}", sessionId);

            var result = await _preShipmentService.DeleteSessionAsync(sessionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PRE-SHIPMENT] Error deleting session - SessionId: {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Internal server error", "An unexpected error occurred"));
        }
    }
}
