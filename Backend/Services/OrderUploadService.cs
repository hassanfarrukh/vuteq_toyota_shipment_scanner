// Author: Hassan
// Date: 2025-12-04
// Description: Service for Order Upload operations - handles Excel upload, parsing, and storage

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Order Upload service operations
/// </summary>
public interface IOrderUploadService
{
    Task<ApiResponse<OrderUploadResponseDto>> UploadAndProcessFileAsync(IFormFile file, Guid userId);
    Task<ApiResponse<IEnumerable<OrderUploadResponseDto>>> GetUploadHistoryAsync();
    Task<ApiResponse<OrderUploadResponseDto>> GetUploadByIdAsync(Guid id);
    Task<ApiResponse<bool>> DeleteUploadAsync(Guid id);
}

/// <summary>
/// Service implementation for Order Upload operations
/// </summary>
public class OrderUploadService : IOrderUploadService
{
    private readonly IOrderUploadRepository _uploadRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IExcelParserService _excelParserService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<OrderUploadService> _logger;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly HashSet<string> AllowedFileTypes = new HashSet<string>
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // .xlsx
    };
    private const string UploadDirectory = "wwwroot/uploads/orders";

    public OrderUploadService(
        IOrderUploadRepository uploadRepository,
        IOrderRepository orderRepository,
        IExcelParserService excelParserService,
        IWebHostEnvironment environment,
        ILogger<OrderUploadService> logger)
    {
        _uploadRepository = uploadRepository;
        _orderRepository = orderRepository;
        _excelParserService = excelParserService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Upload Excel file, parse it, and store orders in database
    /// </summary>
    public async Task<ApiResponse<OrderUploadResponseDto>> UploadAndProcessFileAsync(
        IFormFile file, Guid userId)
    {
        OrderUpload? uploadRecord = null;

        try
        {
            // Validate file
            var validationError = ValidateFile(file);
            if (validationError != null)
            {
                return ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                    "File validation failed", validationError);
            }

            _logger.LogInformation("Starting file upload: {FileName} ({FileSize} bytes) by user {UserId}",
                file.FileName, file.Length, userId);

            // Detect file type by extension
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            bool isExcel = fileExtension == ".xlsx";

            _logger.LogInformation("Detected file type: {FileType}", isExcel ? "Excel" : "Unknown");

            // Create upload directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.ContentRootPath, UploadDirectory);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("Created upload directory: {UploadPath}", uploadPath);
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Create upload record (status: processing)
            uploadRecord = new OrderUpload
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FileSize = file.Length,
                FilePath = Path.Combine(UploadDirectory, uniqueFileName),
                Status = "processing",
                UploadedBy = userId,
                UploadDate = DateTime.UtcNow
            };

            await _uploadRepository.CreateUploadAsync(uploadRecord);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved to: {FilePath}", filePath);

            // Parse Excel file
            List<ExtractedOrderDto> extractedOrders;
            NamcSummary? namcSummary = null;

            if (!isExcel)
            {
                throw new InvalidOperationException($"Unsupported file type: {fileExtension}. Only Excel (.xlsx) files are allowed.");
            }

            // Parse Excel using ClosedXML
            _logger.LogInformation("Using ClosedXML-based Excel parser...");
            var excelResult = await _excelParserService.ParseComplianceDashboardAsync(filePath);

            // Store NAMC summary data
            namcSummary = excelResult.Summary;
            _logger.LogInformation("Excel NAMC Summary: SupplierCode={SupplierCode}, PlantCode={PlantCode}, Pending={Pending}",
                namcSummary.SupplierCode, namcSummary.PlantCode, namcSummary.TotalPending);

            // Convert ParsedShipment to ExtractedOrderDto for unified processing
            extractedOrders = ConvertExcelShipmentsToOrders(excelResult.Shipments);

            // Count unique manifests from the shipments
            int totalManifests = excelResult.Shipments
                .Select(s => s.ManifestNo)
                .Where(m => m > 0)
                .Distinct()
                .Count();

            _logger.LogInformation("Excel parsed successfully. Extracted {OrderCount} orders from {ManifestCount} manifests",
                extractedOrders.Count, totalManifests);

            // Store orders in database (NEW STRUCTURE V2: one order per OrderNumber)
            int ordersCreated = 0;
            int itemsCreated = 0;
            int ordersSkipped = 0;
            var skippedOrderNumbers = new List<string>();

            foreach (var extractedOrder in extractedOrders)
            {
                // Excel files have RealOrderNumber already set (e.g., "2025120233")
                string realOrderNumber = extractedOrder.RealOrderNumber ?? "UNKNOWN";

                // Check if order already exists (RealOrderNumber + DockCode)
                bool orderExists = await _orderRepository.OrderExistsByOrderNumberAndDockAsync(
                    realOrderNumber, extractedOrder.DockCode!);

                if (orderExists)
                {
                    ordersSkipped++;
                    skippedOrderNumbers.Add(realOrderNumber);

                    _logger.LogWarning("Order {RealOrderNumber} (Dock: {DockCode}) already exists, skipping",
                        realOrderNumber, extractedOrder.DockCode);
                    continue;
                }

                // Create Order entity (ONE row per RealOrderNumber + DockCode)
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    RealOrderNumber = realOrderNumber,
                    TransmitDate = extractedOrder.OrderDate,
                    SupplierCode = extractedOrder.SupplierCode,
                    DockCode = extractedOrder.DockCode!,
                    UploadId = uploadRecord.Id, // Link to the upload that created this order
                    UnloadDate = extractedOrder.UnloadDateTime.HasValue
                        ? DateOnly.FromDateTime(extractedOrder.UnloadDateTime.Value)
                        : null,
                    UnloadTime = extractedOrder.UnloadDateTime.HasValue
                        ? TimeOnly.FromDateTime(extractedOrder.UnloadDateTime.Value)
                        : null,
                    PlannedPickup = extractedOrder.PlannedPickup, // Added 2025-12-09 - PLANNED PICKUP (departure date)
                    Status = Models.Enums.OrderStatus.Planned,
                    // Excel-specific fields
                    PlantCode = extractedOrder.PlantCode,
                    PlannedRoute = extractedOrder.PlannedRoute,
                    MainRoute = extractedOrder.MainRoute,
                    SpecialistCode = extractedOrder.SpecialistCode,
                    Mros = extractedOrder.Mros,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString() // Set CreatedBy to the user who uploaded
                };

                _logger.LogInformation("Creating order - DockCode: '{DockCode}', RealOrderNumber: '{RealOrderNumber}'",
                    order.DockCode, order.RealOrderNumber);

                await _orderRepository.CreateOrderAsync(order);
                ordersCreated++;

                // Create PlannedItem entities (ONE row per Part - simplified)
                var plannedItems = extractedOrder.Items.Select(item => new PlannedItem
                {
                    PlannedItemId = Guid.NewGuid(),
                    OrderId = order.OrderId, // FK to Order GUID
                    PartNumber = item.PartNumber,
                    Qpc = item.Qpc ?? item.LotQty, // Use Qpc if available, fallback to LotQty
                    KanbanNumber = item.KanbanNumber,
                    TotalBoxPlanned = item.TotalBoxPlanned ?? item.PlannedQty, // Use TotalBoxPlanned if available, fallback to PlannedQty
                    ManifestNo = item.ManifestNo, // Each item has its own ManifestNo from Excel row
                    ShortOver = item.ShortOver, // Added 2025-12-09 - SHORT/OVER
                    Pieces = item.Pieces, // Added 2025-12-09 - PIECES
                    PalletizationCode = item.PalletizationCode,
                    ExternalOrderId = item.ExternalOrderId ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString() // Set CreatedBy to the user who uploaded
                }).ToList();

                if (plannedItems.Any())
                {
                    await _orderRepository.CreatePlannedItemsAsync(plannedItems);
                    itemsCreated += plannedItems.Count;
                }

                _logger.LogInformation("Created order {RealOrderNumber} (Dock: {DockCode}) with {ItemCount} items",
                    realOrderNumber, order.DockCode, plannedItems.Count);
            }

            // Determine final status based on results
            string finalStatus;
            if (ordersCreated == 0 && ordersSkipped > 0)
            {
                // All orders were skipped (duplicates) - warning status
                finalStatus = "warning";
            }
            else if (ordersCreated == 0 && ordersSkipped == 0)
            {
                // No orders extracted and none skipped - warning/error status
                finalStatus = "warning";
            }
            else
            {
                // Orders were created successfully
                finalStatus = "success";
            }

            // Update upload status with actual counts and NAMC summary
            if (namcSummary != null)
            {
                // Update with NAMC summary data
                uploadRecord.SupplierCode = namcSummary.SupplierCode;
                uploadRecord.PlantCode = namcSummary.PlantCode;
                uploadRecord.TotalPlanned = namcSummary.TotalPlanned;
                uploadRecord.TotalShipped = namcSummary.TotalShipped;
                uploadRecord.TotalShorted = namcSummary.TotalShorted;
                uploadRecord.TotalLate = namcSummary.TotalLate;
                uploadRecord.TotalPending = namcSummary.TotalPending;
                uploadRecord.Status = finalStatus;
                uploadRecord.OrdersCreated = ordersCreated;
                uploadRecord.TotalItemsCreated = itemsCreated;
                uploadRecord.TotalManifestsCreated = totalManifests;

                await _uploadRepository.UpdateUploadAsync(uploadRecord);
            }

            _logger.LogInformation("Order upload status updated: {UploadId} -> {Status} ({OrderCount} orders, {ItemCount} items, {SkippedCount} skipped)",
                uploadRecord.Id, finalStatus, ordersCreated, itemsCreated, ordersSkipped);

            // Build response
            var response = new OrderUploadResponseDto
            {
                UploadId = uploadRecord.Id,
                FileName = uploadRecord.FileName,
                FileSize = uploadRecord.FileSize,
                UploadDate = uploadRecord.UploadDate,
                Status = "success",
                OrdersCreated = ordersCreated,
                TotalItemsCreated = itemsCreated,
                TotalManifestsCreated = totalManifests,
                OrdersSkipped = ordersSkipped,
                SkippedOrderNumbers = skippedOrderNumbers,
                ExtractedOrders = extractedOrders
            };

            // Determine response message and status based on results
            string message;
            if (ordersCreated == 0 && ordersSkipped > 0)
            {
                // All orders were skipped - return warning response
                response.Status = "warning";
                message = $"All {ordersSkipped} order(s) already exist in the system. No new orders were created. " +
                         $"Skipped orders: {string.Join(", ", skippedOrderNumbers)}.";

                return ApiResponse<OrderUploadResponseDto>.WarningResponse(
                    response,
                    message);
            }
            else if (ordersCreated > 0 && ordersSkipped > 0)
            {
                // Some orders created, some skipped - return success with info
                message = $"Created {ordersCreated} order(s) with {itemsCreated} item(s). " +
                         $"Skipped {ordersSkipped} order(s) (already exist): {string.Join(", ", skippedOrderNumbers)}.";
            }
            else
            {
                // All orders created successfully
                message = $"Successfully uploaded and processed {file.FileName}. " +
                         $"Created {ordersCreated} order(s) with {itemsCreated} item(s).";
            }

            return ApiResponse<OrderUploadResponseDto>.SuccessResponse(response, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file upload: {FileName}", file.FileName);

            // Update upload status to error if record was created
            if (uploadRecord != null)
            {
                try
                {
                    await _uploadRepository.UpdateUploadStatusAsync(
                        uploadRecord.Id, "error", ex.Message);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Error updating upload status to error");
                }
            }

            return ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                "Failed to process file upload",
                ex.Message);
        }
    }

    /// <summary>
    /// Get all upload history
    /// </summary>
    public async Task<ApiResponse<IEnumerable<OrderUploadResponseDto>>> GetUploadHistoryAsync()
    {
        try
        {
            var uploads = await _uploadRepository.GetAllUploadsAsync();

            var uploadDtos = uploads.Select(u => new OrderUploadResponseDto
            {
                UploadId = u.Id,
                FileName = u.FileName,
                FileSize = u.FileSize,
                UploadDate = u.UploadDate,
                Status = u.Status,
                ErrorMessage = u.ErrorMessage,
                OrdersCreated = u.OrdersCreated,
                TotalItemsCreated = u.TotalItemsCreated,
                TotalManifestsCreated = u.TotalManifestsCreated,
                OrdersSkipped = 0, // Historical records don't have this data
                SkippedOrderNumbers = new List<string>(),
                UploadedByUsername = u.UploadedByUser?.Username
            });

            return ApiResponse<IEnumerable<OrderUploadResponseDto>>.SuccessResponse(
                uploadDtos,
                "Upload history retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upload history");
            return ApiResponse<IEnumerable<OrderUploadResponseDto>>.ErrorResponse(
                "Failed to retrieve upload history",
                ex.Message);
        }
    }

    /// <summary>
    /// Get specific upload by ID
    /// </summary>
    public async Task<ApiResponse<OrderUploadResponseDto>> GetUploadByIdAsync(Guid id)
    {
        try
        {
            var upload = await _uploadRepository.GetUploadByIdAsync(id);
            if (upload == null)
            {
                return ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                    "Upload not found",
                    $"Upload with ID '{id}' does not exist");
            }

            var uploadDto = new OrderUploadResponseDto
            {
                UploadId = upload.Id,
                FileName = upload.FileName,
                FileSize = upload.FileSize,
                UploadDate = upload.UploadDate,
                Status = upload.Status,
                ErrorMessage = upload.ErrorMessage,
                OrdersCreated = upload.OrdersCreated,
                TotalItemsCreated = upload.TotalItemsCreated,
                TotalManifestsCreated = upload.TotalManifestsCreated,
                OrdersSkipped = 0, // Historical records don't have this data
                SkippedOrderNumbers = new List<string>(),
                UploadedByUsername = upload.UploadedByUser?.Username
            };

            return ApiResponse<OrderUploadResponseDto>.SuccessResponse(
                uploadDto,
                "Upload retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upload: {UploadId}", id);
            return ApiResponse<OrderUploadResponseDto>.ErrorResponse(
                "Failed to retrieve upload",
                ex.Message);
        }
    }

    /// <summary>
    /// Delete upload record and associated file
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteUploadAsync(Guid id)
    {
        try
        {
            var upload = await _uploadRepository.GetUploadByIdAsync(id);
            if (upload == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Upload not found",
                    $"Upload with ID '{id}' does not exist");
            }

            // Delete physical file if it exists
            if (!string.IsNullOrEmpty(upload.FilePath))
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, upload.FilePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted file: {FilePath}", fullPath);
                }
            }

            // Delete upload record
            var deleted = await _uploadRepository.DeleteUploadAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Upload deleted: {UploadId}", id);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Upload deleted successfully");
            }

            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete upload",
                "Unknown error occurred");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting upload: {UploadId}", id);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete upload",
                ex.Message);
        }
    }

    /// <summary>
    /// Convert Excel shipments to ExtractedOrderDto for unified processing
    /// Groups shipments by RealOrderNumber + DockCode (one Order per combination)
    /// </summary>
    private List<ExtractedOrderDto> ConvertExcelShipmentsToOrders(List<ParsedShipment> shipments)
    {
        var orders = new List<ExtractedOrderDto>();

        // Group shipments by RealOrderNumber + DockCode
        var groupedShipments = shipments
            .GroupBy(s => new { s.RealOrderNumber, s.DockCode })
            .ToList();

        _logger.LogInformation("Grouping {ShipmentCount} shipments into {GroupCount} orders (by RealOrderNumber + DockCode)",
            shipments.Count, groupedShipments.Count);

        foreach (var group in groupedShipments)
        {
            // Take order-level data from first shipment in group
            var firstShipment = group.First();

            // Create ExtractedOrderDto
            var order = new ExtractedOrderDto
            {
                // Order identification
                RealOrderNumber = firstShipment.RealOrderNumber, // From ORDER NUMBER
                DockCode = firstShipment.DockCode,
                SupplierCode = firstShipment.SupplierCode,

                // Dates
                OrderDate = firstShipment.TransmitDate, // ORDER DATE → TransmitDate
                UnloadDateTime = firstShipment.UnloadDate.HasValue && firstShipment.UnloadTime.HasValue
                    ? firstShipment.UnloadDate.Value.ToDateTime(firstShipment.UnloadTime.Value)
                    : null,
                PlannedPickup = firstShipment.PlannedPickup, // Added 2025-12-09 - PLANNED PICKUP

                // Excel-specific fields
                ManifestNo = firstShipment.ManifestNo,
                PlantCode = firstShipment.PlantCode,
                PlannedRoute = firstShipment.PlannedRoute,
                MainRoute = firstShipment.MainRoute,
                SpecialistCode = firstShipment.SpecialistCode,
                Mros = firstShipment.Mros,

                // Items - one per PART/KANBAN from this manifest
                Items = group.Select(s => new ExtractedOrderItemDto
                {
                    PartNumber = s.PartNumber,
                    KanbanNumber = s.KanbanNumber,
                    Qpc = s.Qpc, // QPC
                    TotalBoxPlanned = s.TotalBoxPlanned, // TOTAL BOX PLANNED
                    ManifestNo = s.ManifestNo, // Each item has its own ManifestNo
                    PalletizationCode = s.PalletizationCode,
                    ExternalOrderId = s.ExternalOrderId, // ORDER ID
                    ShortOver = s.ShortOver, // Added 2025-12-09 - SHORT/OVER
                    Pieces = s.Pieces // Added 2025-12-09 - PIECES
                }).ToList()
            };

            orders.Add(order);

            _logger.LogDebug("Created order for Manifest={ManifestNo}, Dock={DockCode}, RealOrderNumber={RealOrderNumber}, Items={ItemCount}",
                firstShipment.ManifestNo, firstShipment.DockCode, order.RealOrderNumber, order.Items.Count);
        }

        return orders;
    }

    /// <summary>
    /// Validate uploaded file (Excel only)
    /// </summary>
    private string? ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return "File is required";
        }

        if (file.Length > MaxFileSize)
        {
            return $"File size must be less than {MaxFileSize / 1024 / 1024}MB";
        }

        if (!AllowedFileTypes.Contains(file.ContentType))
        {
            return "Only Excel (.xlsx) files are allowed";
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return "Only Excel (.xlsx) files are allowed";
        }

        return null;
    }
}
