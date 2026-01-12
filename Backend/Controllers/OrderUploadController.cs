// Author: Hassan
// Date: 2025-12-01
// Description: Controller for Order Upload operations - handles PDF file uploads and processing

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Controller for order upload operations
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrderUploadController : ControllerBase
{
    private readonly IOrderUploadService _orderUploadService;
    private readonly IPlannedItemService _plannedItemService;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderUploadController> _logger;

    public OrderUploadController(
        IOrderUploadService orderUploadService,
        IPlannedItemService plannedItemService,
        IOrderService orderService,
        ILogger<OrderUploadController> logger)
    {
        _orderUploadService = orderUploadService;
        _plannedItemService = plannedItemService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Upload and process order PDF file
    /// </summary>
    /// <remarks>
    /// Upload a PDF file containing TMMI Daily One-Way Kanban Order Summary Report.
    /// The PDF will be parsed and order data will be extracted and stored in the database.
    ///
    /// **File Requirements:**
    /// - File type: PDF only
    /// - Max size: 10MB
    /// - Format: TMMI Daily One-Way Kanban Order Summary Report
    ///
    /// **Parser:**
    /// - Uses PdfPig text extraction for PDF parsing
    ///
    /// **Sample Request:**
    /// ```
    /// POST /api/v1/orders/upload
    /// Content-Type: multipart/form-data
    ///
    /// file: [PDF file]
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Successfully uploaded and processed file.pdf. Created 3 orders with 15 items.",
    ///   "data": {
    ///     "uploadId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "fileName": "order_20251117.pdf",
    ///     "fileSize": 2048576,
    ///     "uploadDate": "2025-12-01T10:30:00Z",
    ///     "status": "success",
    ///     "ordersCreated": 3,
    ///     "totalItemsCreated": 15,
    ///     "extractedOrders": [...]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="file">PDF file to upload</param>
    /// <returns>Upload response with extracted order data</returns>
    /// <response code="200">File uploaded and processed successfully</response>
    /// <response code="400">Invalid file or validation error</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error during processing</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<OrderUploadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderUploadResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadOrderFile([FromForm] IFormFile file)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT token");
                return Unauthorized(ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                    "Authentication failed",
                    "Invalid user credentials"));
            }

            _logger.LogInformation("Upload request received from user {UserId}", userId);

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                    "File validation failed",
                    "File is required"));
            }

            // Process upload
            var result = await _orderUploadService.UploadAndProcessFileAsync(file, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in upload endpoint");
            return StatusCode(500, ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Get all orders with TotalParts count
    /// </summary>
    /// <remarks>
    /// Retrieve all orders with their total parts count.
    /// Optionally filter by upload ID or date range (based on upload date).
    ///
    /// **Query Parameters:**
    /// - uploadId (optional): Filter orders by upload ID
    /// - fromDate (optional): Start date for filtering by upload date (ISO 8601 format, e.g., 2026-01-01)
    /// - toDate (optional): End date for filtering by upload date (ISO 8601 format, e.g., 2026-01-31, inclusive)
    ///
    /// **Sample Request (all orders):**
    /// ```
    /// GET /api/v1/orders
    /// ```
    ///
    /// **Sample Request (filtered by upload):**
    /// ```
    /// GET /api/v1/orders?uploadId=550e8400-e29b-41d4-a716-446655440000
    /// ```
    ///
    /// **Sample Request (filtered by date range):**
    /// ```
    /// GET /api/v1/orders?fromDate=2026-01-01&amp;toDate=2026-01-31
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Retrieved 23 order(s)",
    ///   "data": [
    ///     {
    ///       "orderId": "220e8400-e29b-41d4-a716-446655440222",
    ///       "realOrderNumber": "2025111701",
    ///       "totalParts": 15,
    ///       "dockCode": "FL",
    ///       "departureDate": "2025-11-18T08:30:00Z",
    ///       "orderDate": "2025-11-17T10:00:00Z",
    ///       "status": "Planned",
    ///       "uploadId": "550e8400-e29b-41d4-a716-446655440000",
    ///       "plannedRoute": "IDRE-06",
    ///       "mainRoute": "IEH6-33"
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="uploadId">Optional upload ID to filter results</param>
    /// <param name="fromDate">Optional start date for filtering by upload date (inclusive)</param>
    /// <param name="toDate">Optional end date for filtering by upload date (inclusive)</param>
    /// <returns>List of orders with total parts count</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] Guid? uploadId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _orderService.GetOrdersAsync(uploadId, fromDate, toDate);

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving orders. UploadId: {UploadId}, FromDate: {FromDate}, ToDate: {ToDate}",
                uploadId, fromDate, toDate);
            return StatusCode(500, ApiResponse<IEnumerable<OrderListDto>>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving orders"));
        }
    }

    /// <summary>
    /// Get upload history with optional date range filter
    /// </summary>
    /// <remarks>
    /// Retrieve all order file upload history, ordered by upload date (newest first).
    /// Optionally filter by date range using fromDate and toDate query parameters.
    ///
    /// **Query Parameters:**
    /// - fromDate (optional): Start date for filtering (ISO 8601 format, e.g., 2026-01-01)
    /// - toDate (optional): End date for filtering (ISO 8601 format, e.g., 2026-01-31)
    ///
    /// **Sample Request (all uploads):**
    /// ```
    /// GET /api/v1/orders/uploads
    /// ```
    ///
    /// **Sample Request (filtered by date range):**
    /// ```
    /// GET /api/v1/orders/uploads?fromDate=2026-01-01&amp;toDate=2026-01-31
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Upload history retrieved successfully (15 records)",
    ///   "data": [
    ///     {
    ///       "uploadId": "550e8400-e29b-41d4-a716-446655440000",
    ///       "fileName": "order_20251117.xlsx",
    ///       "fileSize": 2048576,
    ///       "uploadDate": "2026-01-12T10:30:00Z",
    ///       "status": "success",
    ///       "ordersCreated": 10,
    ///       "totalItemsCreated": 150,
    ///       "uploadedByUsername": "admin"
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="fromDate">Optional start date for filtering (inclusive)</param>
    /// <param name="toDate">Optional end date for filtering (inclusive)</param>
    /// <returns>List of all upload records matching the date range</returns>
    /// <response code="200">Upload history retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("uploads")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderUploadResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUploadHistory(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _orderUploadService.GetUploadHistoryAsync(fromDate, toDate);

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving upload history. FromDate: {FromDate}, ToDate: {ToDate}",
                fromDate, toDate);
            return StatusCode(500, ApiResponse<IEnumerable<OrderUploadResponseDto>>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving upload history"));
        }
    }

    /// <summary>
    /// Get specific upload by ID
    /// </summary>
    /// <remarks>
    /// Retrieve details of a specific order file upload by its ID.
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Upload retrieved successfully",
    ///   "data": {
    ///     "uploadId": "550e8400-e29b-41d4-a716-446655440000",
    ///     "fileName": "order_20251117.pdf",
    ///     "fileSize": 2048576,
    ///     "uploadDate": "2025-12-01T10:30:00Z",
    ///     "status": "success"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Upload ID</param>
    /// <returns>Upload record details</returns>
    /// <response code="200">Upload retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Upload not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("uploads/{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderUploadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<OrderUploadResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUploadById(Guid id)
    {
        try
        {
            var result = await _orderUploadService.GetUploadByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving upload {UploadId}", id);
            return StatusCode(500, ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving upload"));
        }
    }

    /// <summary>
    /// Delete upload record
    /// </summary>
    /// <remarks>
    /// Delete an order file upload record and its associated physical file.
    /// Note: This does NOT delete the orders that were created from this upload.
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Upload deleted successfully",
    ///   "data": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Upload ID to delete</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Upload deleted successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Upload not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("uploads/{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUpload(Guid id)
    {
        try
        {
            var result = await _orderUploadService.DeleteUploadAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting upload {UploadId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while deleting upload"));
        }
    }

    /// <summary>
    /// Get all planned items with order information
    /// </summary>
    /// <remarks>
    /// Retrieve all planned items with their associated order information.
    /// Optionally filter by upload ID, order ID, or date range (based on upload date).
    ///
    /// **Query Parameters:**
    /// - uploadId (optional): Filter planned items by upload ID
    /// - orderId (optional): Filter planned items by order ID (takes precedence over uploadId)
    /// - fromDate (optional): Start date for filtering by upload date (ISO 8601 format, e.g., 2026-01-01)
    /// - toDate (optional): End date for filtering by upload date (ISO 8601 format, e.g., 2026-01-31, inclusive)
    ///
    /// **Sample Request (all items):**
    /// ```
    /// GET /api/v1/orders/planned-items
    /// ```
    ///
    /// **Sample Request (filtered by upload):**
    /// ```
    /// GET /api/v1/orders/planned-items?uploadId=550e8400-e29b-41d4-a716-446655440000
    /// ```
    ///
    /// **Sample Request (filtered by order):**
    /// ```
    /// GET /api/v1/orders/planned-items?orderId=220e8400-e29b-41d4-a716-446655440222
    /// ```
    ///
    /// **Sample Request (filtered by date range):**
    /// ```
    /// GET /api/v1/orders/planned-items?fromDate=2026-01-01&amp;toDate=2026-01-31
    /// ```
    ///
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Retrieved 150 planned item(s)",
    ///   "data": [
    ///     {
    ///       "plannedItemId": "110e8400-e29b-41d4-a716-446655440111",
    ///       "orderId": "220e8400-e29b-41d4-a716-446655440222",
    ///       "realOrderNumber": "2025111701",
    ///       "dockCode": "FL",
    ///       "partNumber": "68101-0E120-00",
    ///       "partDescription": "GLASS, WINDSHIELD",
    ///       "lotQty": 10,
    ///       "kanbanNumber": "FA99",
    ///       "internalKanban": "1234567890A1ABCDE9999999",
    ///       "lotOrdered": 100,
    ///       "createdAt": "2025-12-01T10:30:00Z"
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="uploadId">Optional upload ID to filter results</param>
    /// <param name="orderId">Optional order ID to filter results (takes precedence over uploadId)</param>
    /// <param name="fromDate">Optional start date for filtering by upload date (inclusive)</param>
    /// <param name="toDate">Optional end date for filtering by upload date (inclusive)</param>
    /// <returns>List of planned items with order information</returns>
    /// <response code="200">Planned items retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("planned-items")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlannedItemWithOrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlannedItems(
        [FromQuery] Guid? uploadId = null,
        [FromQuery] Guid? orderId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _plannedItemService.GetPlannedItemsAsync(uploadId, orderId, fromDate, toDate);

            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving planned items. UploadId: {UploadId}, OrderId: {OrderId}, FromDate: {FromDate}, ToDate: {ToDate}",
                uploadId, orderId, fromDate, toDate);
            return StatusCode(500, ApiResponse<IEnumerable<PlannedItemWithOrderDto>>.ErrorResponse(
                "Internal server error",
                "An unexpected error occurred while retrieving planned items"));
        }
    }
}
