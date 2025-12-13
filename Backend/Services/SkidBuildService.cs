// Author: Hassan
// Date: 2025-12-06
// Updated: 2025-12-13 - Updated to return ScanDetails instead of InternalKanbans
// Updated: 2025-12-14 - Integrated Toyota API submission in CompleteSessionAsync
// Description: Service for Skid Build operations - handles business logic and Toyota API integration

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Skid Build service operations
/// </summary>
public interface ISkidBuildService
{
    Task<ApiResponse<SkidBuildOrderDto>> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode);
    Task<ApiResponse<SkidBuildOrderGroupedDto>> GetOrderByNumberAndDockGroupedAsync(string orderNumber, string dockCode);
    Task<ApiResponse<SkidBuildSessionDto>> StartSessionAsync(SkidBuildStartSessionRequestDto request);
    Task<ApiResponse<SkidBuildScanResponseDto>> RecordScanAsync(SkidBuildScanRequestDto request);
    Task<ApiResponse<SkidBuildException>> RecordExceptionAsync(SkidBuildExceptionRequestDto request);
    Task<ApiResponse<bool>> DeleteExceptionAsync(Guid exceptionId);
    Task<ApiResponse<SkidBuildCompleteResponseDto>> CompleteSessionAsync(Guid sessionId, Guid userId);
    Task<ApiResponse<SkidBuildSessionDto>> GetSessionByIdAsync(Guid sessionId);
}

/// <summary>
/// Service implementation for Skid Build operations
/// </summary>
public class SkidBuildService : ISkidBuildService
{
    private readonly ISkidBuildRepository _skidBuildRepository;
    private readonly IToyotaValidationService _toyotaValidationService;
    private readonly IToyotaApiService _toyotaApiService;
    private readonly ILogger<SkidBuildService> _logger;

    // System user ID for operations when user is not authenticated
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public SkidBuildService(
        ISkidBuildRepository skidBuildRepository,
        IToyotaValidationService toyotaValidationService,
        IToyotaApiService toyotaApiService,
        ILogger<SkidBuildService> logger)
    {
        _skidBuildRepository = skidBuildRepository;
        _toyotaValidationService = toyotaValidationService;
        _toyotaApiService = toyotaApiService;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the user ID from the request or uses system default
    /// Parses string userId to Guid, returns system user if null/empty/invalid
    /// </summary>
    private Guid ResolveUserId(string? userId)
    {
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedGuid))
        {
            return parsedGuid;
        }
        return SystemUserId;
    }

    /// <summary>
    /// Get order by order number and dock code with planned items
    /// </summary>
    public async Task<ApiResponse<SkidBuildOrderDto>> GetOrderByNumberAndDockAsync(string orderNumber, string dockCode)
    {
        try
        {
            var order = await _skidBuildRepository.GetOrderByNumberAndDockAsync(orderNumber, dockCode);

            if (order == null)
            {
                return ApiResponse<SkidBuildOrderDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with number {orderNumber} and dock code {dockCode}");
            }

            // Get scanned counts and scan details for each planned item
            var plannedItemDtos = new List<SkidBuildPlannedItemDto>();
            foreach (var item in order.PlannedItems)
            {
                var scannedCount = await _skidBuildRepository.GetScannedCountByPlannedItemAsync(item.PlannedItemId);
                var scanDetails = await _skidBuildRepository.GetScanDetailsByPlannedItemAsync(item.PlannedItemId);

                plannedItemDtos.Add(new SkidBuildPlannedItemDto
                {
                    PlannedItemId = item.PlannedItemId,
                    PartNumber = item.PartNumber,
                    KanbanNumber = item.KanbanNumber,
                    Qpc = item.Qpc,
                    TotalBoxPlanned = item.TotalBoxPlanned,
                    ManifestNo = item.ManifestNo,
                    PalletizationCode = item.PalletizationCode,
                    ScannedCount = scannedCount,
                    ScanDetails = scanDetails
                });
            }

            var orderDto = new SkidBuildOrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                SupplierCode = order.SupplierCode,
                PlantCode = order.PlantCode,
                Status = order.Status.ToString(),
                PlannedItems = plannedItemDtos
            };

            return ApiResponse<SkidBuildOrderDto>.SuccessResponse(
                orderDto,
                $"Order {orderNumber} retrieved successfully with {plannedItemDtos.Count} planned items");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            return ApiResponse<SkidBuildOrderDto>.ErrorResponse(
                "Failed to retrieve order",
                ex.Message);
        }
    }

    /// <summary>
    /// Get order by order number and dock code with planned items grouped by skid (ManifestNo)
    /// </summary>
    public async Task<ApiResponse<SkidBuildOrderGroupedDto>> GetOrderByNumberAndDockGroupedAsync(string orderNumber, string dockCode)
    {
        try
        {
            var order = await _skidBuildRepository.GetOrderByNumberAndDockAsync(orderNumber, dockCode);

            if (order == null)
            {
                return ApiResponse<SkidBuildOrderGroupedDto>.ErrorResponse(
                    "Order not found",
                    $"No order found with number {orderNumber} and dock code {dockCode}");
            }

            // Get scanned counts and scan details for each planned item and prepare DTOs
            var plannedItemDtos = new List<SkidBuildPlannedItemDto>();
            foreach (var item in order.PlannedItems)
            {
                var scannedCount = await _skidBuildRepository.GetScannedCountByPlannedItemAsync(item.PlannedItemId);
                var scanDetails = await _skidBuildRepository.GetScanDetailsByPlannedItemAsync(item.PlannedItemId);

                plannedItemDtos.Add(new SkidBuildPlannedItemDto
                {
                    PlannedItemId = item.PlannedItemId,
                    PartNumber = item.PartNumber,
                    KanbanNumber = item.KanbanNumber,
                    Qpc = item.Qpc,
                    TotalBoxPlanned = item.TotalBoxPlanned,
                    ManifestNo = item.ManifestNo,
                    PalletizationCode = item.PalletizationCode,
                    ScannedCount = scannedCount,
                    ScanDetails = scanDetails
                });
            }

            // Group by ManifestNo
            var groupedByManifest = plannedItemDtos
                .GroupBy(x => x.ManifestNo)
                .OrderBy(g => g.Key)
                .ToList();

            // Create SkidGroupDto list
            var skidGroups = new List<SkidGroupDto>();
            foreach (var group in groupedByManifest)
            {
                // Generate SkidId from ManifestNo: last 3 digits + A suffix
                var manifestNoStr = group.Key.ToString();
                var last3Digits = manifestNoStr.Length >= 3
                    ? manifestNoStr.Substring(manifestNoStr.Length - 3)
                    : manifestNoStr.PadLeft(3, '0');
                var skidId = $"{last3Digits}A";

                // Get PalletizationCode from the first item in the group (should be same for all)
                var palletizationCode = group.FirstOrDefault()?.PalletizationCode;

                skidGroups.Add(new SkidGroupDto
                {
                    SkidId = skidId,
                    ManifestNo = group.Key,
                    PalletizationCode = palletizationCode,
                    PlannedKanbans = group.ToList()
                });
            }

            var orderGroupedDto = new SkidBuildOrderGroupedDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.RealOrderNumber,
                DockCode = order.DockCode,
                SupplierCode = order.SupplierCode,
                PlantCode = order.PlantCode,
                Status = order.Status.ToString(),
                Skids = skidGroups,
                // Toyota Skid Build API Fields
                ToyotaSkidBuildConfirmationNumber = order.ToyotaSkidBuildConfirmationNumber,
                ToyotaSkidBuildStatus = order.ToyotaSkidBuildStatus,
                ToyotaSkidBuildErrorMessage = order.ToyotaSkidBuildErrorMessage,
                ToyotaSkidBuildSubmittedAt = order.ToyotaSkidBuildSubmittedAt
            };

            _logger.LogInformation("Order {OrderNumber} retrieved with {SkidCount} skids and {ItemCount} total items",
                orderNumber, skidGroups.Count, plannedItemDtos.Count);

            return ApiResponse<SkidBuildOrderGroupedDto>.SuccessResponse(
                orderGroupedDto,
                $"Order {orderNumber} retrieved successfully with {skidGroups.Count} skids and {plannedItemDtos.Count} total items");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grouped order: {OrderNumber}-{DockCode}", orderNumber, dockCode);
            return ApiResponse<SkidBuildOrderGroupedDto>.ErrorResponse(
                "Failed to retrieve grouped order",
                ex.Message);
        }
    }

    /// <summary>
    /// Start a new skid build session
    /// </summary>
    public async Task<ApiResponse<SkidBuildSessionDto>> StartSessionAsync(SkidBuildStartSessionRequestDto request)
    {
        try
        {
            // Verify order exists by ID
            var order = await _skidBuildRepository.GetOrderByIdAsync(request.OrderId);

            if (order == null)
            {
                return ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                    "Order not found",
                    $"Order {request.OrderId} does not exist");
            }

            // Resolve user ID (use system user if not provided)
            var resolvedUserId = ResolveUserId(request.UserId);

            // Create session
            var session = new SkidBuildSession
            {
                SessionId = Guid.NewGuid(),
                UserId = resolvedUserId,
                OrderId = request.OrderId,
                Status = "active",
                CurrentScreen = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createdSession = await _skidBuildRepository.CreateSessionAsync(session);

            var sessionDto = new SkidBuildSessionDto
            {
                SessionId = createdSession.SessionId,
                OrderId = createdSession.OrderId,
                SkidNumber = request.SkidNumber,
                Status = createdSession.Status,
                UserId = createdSession.UserId,
                CreatedAt = createdSession.CreatedAt,
                CompletedAt = createdSession.CompletedAt,
                ConfirmationNumber = null
            };

            _logger.LogInformation("Skid build session started: {SessionId} for Order: {OrderId}",
                session.SessionId, request.OrderId);

            return ApiResponse<SkidBuildSessionDto>.SuccessResponse(
                sessionDto,
                $"Session started successfully for skid #{request.SkidNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting skid build session for Order: {OrderId}", request.OrderId);
            return ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                "Failed to start session",
                ex.Message);
        }
    }

    /// <summary>
    /// Record a scan (Toyota Kanban + Internal Kanban pair)
    /// </summary>
    public async Task<ApiResponse<SkidBuildScanResponseDto>> RecordScanAsync(SkidBuildScanRequestDto request)
    {
        try
        {
            // Verify session exists
            var session = await _skidBuildRepository.GetSessionByIdAsync(request.SessionId);
            if (session == null)
            {
                return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                    "Session not found",
                    $"Session {request.SessionId} does not exist");
            }

            if (session.Status != "active")
            {
                return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                    "Invalid session state",
                    $"Session {request.SessionId} is not active (status: {session.Status})");
            }

            // TOYOTA VALIDATION: Validate SkidId (must be 3 numeric digits)
            var skidIdValidation = _toyotaValidationService.ValidateSkidId(request.SkidNumber);
            if (!skidIdValidation.IsValid)
            {
                return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                    "Invalid Skid ID",
                    skidIdValidation.ErrorMessage ?? "Skid ID validation failed");
            }

            // TOYOTA VALIDATION: Validate BoxNumber (1-999)
            var boxNumberValidation = _toyotaValidationService.ValidateBoxNumber(request.BoxNumber);
            if (!boxNumberValidation.IsValid)
            {
                return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                    "Invalid Box Number",
                    boxNumberValidation.ErrorMessage ?? "Box number validation failed");
            }

            // TOYOTA VALIDATION: Validate SkidSide if provided (must be A or B)
            if (!string.IsNullOrWhiteSpace(request.SkidSide))
            {
                if (request.SkidSide != "A" && request.SkidSide != "B")
                {
                    return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                        "Invalid Skid Side",
                        "Skid side must be 'A' or 'B'");
                }
            }

            // Resolve user ID (use system user if not provided)
            var resolvedUserId = ResolveUserId(request.UserId);

            // Create scan record
            var scan = new SkidScan
            {
                ScanId = Guid.NewGuid(),
                PlannedItemId = request.PlannedItemId,
                SkidNumber = request.SkidNumber,
                SkidSide = request.SkidSide,
                RawSkidId = request.RawSkidId,
                BoxNumber = request.BoxNumber,
                LineSideAddress = request.LineSideAddress,
                InternalKanban = request.InternalKanban,
                PalletizationCode = request.PalletizationCode,
                ScannedAt = DateTime.UtcNow,
                ScannedBy = resolvedUserId,
                CreatedAt = DateTime.UtcNow
            };

            var createdScan = await _skidBuildRepository.CreateScanAsync(scan);

            // Map to DTO to prevent circular reference
            var responseDto = new SkidBuildScanResponseDto
            {
                ScanId = createdScan.ScanId,
                PlannedItemId = createdScan.PlannedItemId,
                SkidNumber = createdScan.SkidNumber,
                BoxNumber = createdScan.BoxNumber,
                LineSideAddress = createdScan.LineSideAddress,
                InternalKanban = createdScan.InternalKanban,
                ScannedAt = createdScan.ScannedAt,
                ScannedBy = createdScan.ScannedBy
            };

            _logger.LogInformation("Scan recorded: {ScanId} for PlannedItem: {PlannedItemId}, Skid: {SkidNumber}, Box: {BoxNumber}",
                scan.ScanId, request.PlannedItemId, request.SkidNumber, request.BoxNumber);

            return ApiResponse<SkidBuildScanResponseDto>.SuccessResponse(
                responseDto,
                $"Scan recorded successfully for skid #{request.SkidNumber}, box #{request.BoxNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording scan for Session: {SessionId}", request.SessionId);
            return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                "Failed to record scan",
                ex.Message);
        }
    }

    /// <summary>
    /// Record a skid build exception
    /// </summary>
    public async Task<ApiResponse<SkidBuildException>> RecordExceptionAsync(SkidBuildExceptionRequestDto request)
    {
        try
        {
            // TOYOTA VALIDATION: Validate exception code (must be 10, 11, 12, or 20 for Skid Build)
            var exceptionCodeValidation = _toyotaValidationService.ValidateExceptionCode(
                request.ExceptionCode,
                "skid_build_order");

            if (!exceptionCodeValidation.IsValid)
            {
                return ApiResponse<SkidBuildException>.ErrorResponse(
                    "Invalid Exception Code",
                    exceptionCodeValidation.ErrorMessage ?? "Exception code validation failed");
            }

            // Resolve user ID (use system user if not provided)
            var resolvedUserId = ResolveUserId(request.UserId);

            // Create exception record
            var exception = new SkidBuildException
            {
                ExceptionId = Guid.NewGuid(),
                OrderId = request.OrderId,
                SessionId = request.SessionId, // Link to session if provided
                SkidNumber = request.SkidNumber,
                ExceptionCode = request.ExceptionCode,
                Comments = request.Comments,
                CreatedByUserId = resolvedUserId,
                CreatedAt = DateTime.UtcNow
            };

            var createdException = await _skidBuildRepository.CreateExceptionAsync(exception);

            _logger.LogInformation("Exception recorded: {ExceptionId} for Order: {OrderId}, Code: {ExceptionCode}",
                exception.ExceptionId, request.OrderId, request.ExceptionCode);

            return ApiResponse<SkidBuildException>.SuccessResponse(
                createdException,
                $"Exception '{request.ExceptionCode}' recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording exception for Order: {OrderId}", request.OrderId);
            return ApiResponse<SkidBuildException>.ErrorResponse(
                "Failed to record exception",
                ex.Message);
        }
    }

    /// <summary>
    /// Delete a skid build exception by ID
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteExceptionAsync(Guid exceptionId)
    {
        try
        {
            // Verify exception exists
            var exception = await _skidBuildRepository.GetExceptionByIdAsync(exceptionId);
            if (exception == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Exception not found",
                    $"Exception {exceptionId} does not exist");
            }

            // Delete the exception
            var deleted = await _skidBuildRepository.DeleteExceptionAsync(exceptionId);

            if (deleted)
            {
                _logger.LogInformation("Exception deleted: {ExceptionId}", exceptionId);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    $"Exception {exceptionId} deleted successfully");
            }

            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete exception",
                "Unable to delete exception");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exception: {ExceptionId}", exceptionId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete exception",
                ex.Message);
        }
    }

    /// <summary>
    /// Complete and submit the skid build session
    /// Integrates with Toyota API to submit skid build data
    /// </summary>
    public async Task<ApiResponse<SkidBuildCompleteResponseDto>> CompleteSessionAsync(Guid sessionId, Guid userId)
    {
        try
        {
            // Get session with full data
            var session = await _skidBuildRepository.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                    "Session not found",
                    $"Session {sessionId} does not exist");
            }

            if (session.Status != "active")
            {
                return ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                    "Invalid session state",
                    $"Session {sessionId} is not active (status: {session.Status})");
            }

            // Get scans and exceptions
            var scans = await _skidBuildRepository.GetScansBySessionAsync(sessionId);
            var exceptions = await _skidBuildRepository.GetExceptionsBySessionAsync(sessionId);
            var scansList = scans.ToList();

            // Get order with all data
            if (!session.OrderId.HasValue)
            {
                return ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                    "Invalid session",
                    "Session does not have an associated order");
            }

            var order = await _skidBuildRepository.GetOrderByIdAsync(session.OrderId.Value);
            if (order == null)
            {
                return ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                    "Order not found",
                    $"Order {session.OrderId} does not exist");
            }

            // ===== BUILD TOYOTA API REQUEST =====
            _logger.LogInformation("Building Toyota API request for Order: {OrderNumber}, Scans: {ScanCount}",
                order.RealOrderNumber, scansList.Count);

            // Group scans by skid number
            var skidGroups = scansList
                .GroupBy(s => new { s.SkidNumber, s.PalletizationCode })
                .OrderBy(g => g.Key.SkidNumber)
                .ToList();

            var toyotaSkids = new List<ToyotaSkid>();

            foreach (var skidGroup in skidGroups)
            {
                // Build Toyota Kanban items for this skid
                var toyotaKanbans = new List<ToyotaKanbanItem>();

                foreach (var scan in skidGroup.OrderBy(s => s.BoxNumber))
                {
                    // Get planned item details
                    var plannedItem = order.PlannedItems.FirstOrDefault(pi => pi.PlannedItemId == scan.PlannedItemId);
                    if (plannedItem == null)
                    {
                        _logger.LogWarning("PlannedItem not found for scan: {ScanId}", scan.ScanId);
                        continue;
                    }

                    toyotaKanbans.Add(new ToyotaKanbanItem
                    {
                        LineSideAddress = scan.LineSideAddress ?? "",
                        PartNumber = plannedItem.PartNumber,
                        Kanban = plannedItem.KanbanNumber ?? "",
                        Qpc = plannedItem.Qpc ?? 0,
                        BoxNumber = scan.BoxNumber,
                        ManifestNumber = plannedItem.ManifestNo.ToString(),
                        RfId = null, // RFID not implemented yet
                        KanbanCut = false
                    });
                }

                // Build skid ID from skid number (e.g., "001" + side)
                var skidId = skidGroup.Key.SkidNumber;
                if (skidGroup.Any(s => !string.IsNullOrEmpty(s.SkidSide)))
                {
                    var skidSide = skidGroup.First(s => !string.IsNullOrEmpty(s.SkidSide))?.SkidSide;
                    if (!string.IsNullOrEmpty(skidSide))
                    {
                        skidId = $"{skidGroup.Key.SkidNumber}{skidSide}";
                    }
                }

                toyotaSkids.Add(new ToyotaSkid
                {
                    Palletization = skidGroup.Key.PalletizationCode ?? "",
                    SkidId = skidId,
                    Kanbans = toyotaKanbans,
                    RfidDetails = null // RFID not implemented yet
                });
            }

            // Build Toyota exceptions
            var toyotaExceptions = exceptions
                .Select(e => new ToyotaException
                {
                    ExceptionCode = e.ExceptionCode,
                    Comments = e.Comments
                })
                .ToList();

            // Build Toyota request
            var toyotaRequest = new List<ToyotaSkidBuildRequest>
            {
                new ToyotaSkidBuildRequest
                {
                    Order = order.RealOrderNumber,
                    Supplier = order.SupplierCode ?? "",
                    Plant = order.PlantCode ?? "",
                    Dock = order.DockCode,
                    Exceptions = toyotaExceptions.Count > 0 ? toyotaExceptions : null,
                    Skids = toyotaSkids
                }
            };

            // ===== SUBMIT TO TOYOTA API =====
            // TODO: Make environment configurable (Dev, QA, Prod)
            var environment = "QA"; // Default to QA for testing

            _logger.LogInformation("Submitting to Toyota API - Environment: {Environment}, Order: {OrderNumber}, Skids: {SkidCount}",
                environment, order.RealOrderNumber, toyotaSkids.Count);

            var toyotaResponse = await _toyotaApiService.SubmitSkidBuildAsync(environment, toyotaRequest);

            // ===== UPDATE ORDER WITH TOYOTA RESPONSE =====
            if (toyotaResponse.Success && !string.IsNullOrEmpty(toyotaResponse.ConfirmationNumber))
            {
                order.ToyotaSkidBuildConfirmationNumber = toyotaResponse.ConfirmationNumber;
                order.ToyotaSkidBuildStatus = "confirmed";
                order.ToyotaSkidBuildSubmittedAt = DateTime.UtcNow;
                order.ToyotaSkidBuildErrorMessage = null;

                _logger.LogInformation("Toyota API submission successful - ConfirmationNumber: {ConfirmationNumber}",
                    toyotaResponse.ConfirmationNumber);
            }
            else
            {
                order.ToyotaSkidBuildStatus = "error";
                order.ToyotaSkidBuildErrorMessage = toyotaResponse.ErrorMessage ?? "Unknown error from Toyota API";
                order.ToyotaSkidBuildSubmittedAt = DateTime.UtcNow;

                _logger.LogError("Toyota API submission failed - Code: {Code}, Error: {Error}",
                    toyotaResponse.Code, toyotaResponse.ErrorMessage);
            }

            // Save order changes (Toyota response fields)
            await _skidBuildRepository.UpdateOrderAsync(order);

            // Generate internal reference number: SKB-{timestamp}-{random}
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var random = new Random().Next(1000, 9999);
            var internalReferenceNumber = $"SKB-{timestamp}-{random}";

            // Update session
            session.Status = "completed";
            session.CompletedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            await _skidBuildRepository.UpdateSessionAsync(session);

            // Build response
            var response = new SkidBuildCompleteResponseDto
            {
                ConfirmationNumber = toyotaResponse.ConfirmationNumber ?? internalReferenceNumber,
                SessionId = sessionId,
                TotalScanned = scansList.Count,
                TotalExceptions = exceptions.Count(),
                CompletedAt = session.CompletedAt.Value,
                ToyotaSubmissionStatus = order.ToyotaSkidBuildStatus,
                ToyotaConfirmationNumber = order.ToyotaSkidBuildConfirmationNumber,
                ToyotaErrorMessage = order.ToyotaSkidBuildErrorMessage
            };

            _logger.LogInformation("Session completed: {SessionId}, InternalRef: {InternalReference}, Toyota: {ToyotaStatus}, Scanned: {TotalScanned}, Exceptions: {TotalExceptions}",
                sessionId, internalReferenceNumber, order.ToyotaSkidBuildStatus, response.TotalScanned, response.TotalExceptions);

            // Return success even if Toyota API failed (we still completed the session locally)
            var message = toyotaResponse.Success
                ? $"Skid build completed successfully. Toyota Confirmation: {toyotaResponse.ConfirmationNumber}"
                : $"Skid build completed locally, but Toyota API submission failed: {toyotaResponse.ErrorMessage}";

            return ApiResponse<SkidBuildCompleteResponseDto>.SuccessResponse(response, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session: {SessionId}", sessionId);
            return ApiResponse<SkidBuildCompleteResponseDto>.ErrorResponse(
                "Failed to complete session",
                ex.Message);
        }
    }

    /// <summary>
    /// Get session details with all scans and exceptions
    /// </summary>
    public async Task<ApiResponse<SkidBuildSessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        try
        {
            var session = await _skidBuildRepository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                    "Session not found",
                    $"Session {sessionId} does not exist");
            }

            var sessionDto = new SkidBuildSessionDto
            {
                SessionId = session.SessionId,
                OrderId = session.OrderId,
                Status = session.Status,
                UserId = session.UserId,
                CreatedAt = session.CreatedAt,
                CompletedAt = session.CompletedAt,
                ConfirmationNumber = null
            };

            return ApiResponse<SkidBuildSessionDto>.SuccessResponse(
                sessionDto,
                "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            return ApiResponse<SkidBuildSessionDto>.ErrorResponse(
                "Failed to retrieve session",
                ex.Message);
        }
    }
}
