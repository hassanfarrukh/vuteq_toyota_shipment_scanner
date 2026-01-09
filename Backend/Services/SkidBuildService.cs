// Author: Hassan
// Date: 2025-12-06
// Updated: 2025-12-13 - Updated to return ScanDetails instead of InternalKanbans
// Updated: 2025-12-14 - Integrated Toyota API submission in CompleteSessionAsync
// Updated: 2025-12-22 - Fixed Order.Status to set SkidBuildError when Toyota API fails
// Description: Service for Skid Build operations - handles business logic and Toyota API integration

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Models.Enums;
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
    Task<ApiResponse<RestartSessionResponseDto>> RestartSessionAsync(Guid sessionId);
}

/// <summary>
/// Parsed Internal Kanban data structure
/// Issue #2: Fixed-position format parsing for validation
/// </summary>
public class ParsedInternalKanban
{
    /// <summary>Part Number - Position 1-12 (12 chars)</summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>Kanban Code - Position 13-17 (5 chars, may be space-padded)</summary>
    public string KanbanCode { get; set; } = string.Empty;

    /// <summary>Serial Number - Position 18+ (variable length)</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Whether the parsing was successful</summary>
    public bool IsValid { get; set; }

    /// <summary>Error message if parsing failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Service implementation for Skid Build operations
/// </summary>
public class SkidBuildService : ISkidBuildService
{
    private readonly ISkidBuildRepository _skidBuildRepository;
    private readonly IToyotaValidationService _toyotaValidationService;
    private readonly IToyotaApiService _toyotaApiService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly ILogger<SkidBuildService> _logger;

    // System user ID for operations when user is not authenticated
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public SkidBuildService(
        ISkidBuildRepository skidBuildRepository,
        IToyotaValidationService toyotaValidationService,
        IToyotaApiService toyotaApiService,
        ISiteSettingsRepository siteSettingsRepository,
        ILogger<SkidBuildService> logger)
    {
        _skidBuildRepository = skidBuildRepository;
        _toyotaValidationService = toyotaValidationService;
        _toyotaApiService = toyotaApiService;
        _siteSettingsRepository = siteSettingsRepository;
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
    /// Issue #2: Parse Internal Kanban using fixed-position format
    /// Format: Position 1-12: Part Number (12 chars)
    ///         Position 13-17: Kanban Code (5 chars, may contain space)
    ///         Position 18+: Serial Number (variable)
    /// Example: "627300820100 HM550004771" or "627300820100HM550004771"
    /// </summary>
    private ParsedInternalKanban ParseInternalKanban(string? internalKanban)
    {
        var result = new ParsedInternalKanban();

        if (string.IsNullOrWhiteSpace(internalKanban))
        {
            result.IsValid = false;
            result.ErrorMessage = "Internal Kanban is empty";
            return result;
        }

        // Remove any leading/trailing whitespace
        var kanban = internalKanban.Trim();

        // Minimum length: 12 (part) + 4 (kanban code min) + 1 (serial min) = 17
        if (kanban.Length < 17)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Internal Kanban too short (min 17 chars): '{kanban}'";
            return result;
        }

        // Parse fixed positions
        // Part Number: First 12 characters
        result.PartNumber = kanban.Substring(0, 12);

        // The remaining part contains Kanban Code + Serial
        // Format could be "627300820100 HM550004771" (with space) or "627300820100HM550004771" (no space)
        var remaining = kanban.Substring(12);

        // If there's a space, split by it
        if (remaining.Contains(' '))
        {
            var parts = remaining.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                // First part is Kanban Code, rest is Serial
                result.KanbanCode = parts[0].Trim();
                result.SerialNumber = parts[1].Trim();
            }
            else if (parts.Length == 1)
            {
                // Only one part after split - assume it's KanbanCode+Serial combined
                // Kanban Code is typically 4-5 chars
                var combined = parts[0];
                if (combined.Length >= 5)
                {
                    result.KanbanCode = combined.Substring(0, 4).Trim();
                    result.SerialNumber = combined.Substring(4).Trim();
                }
                else
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Cannot parse Kanban Code and Serial from: '{combined}'";
                    return result;
                }
            }
        }
        else
        {
            // No space - assume fixed positions: Kanban Code is 4-5 chars starting at position 13
            // Typical format: 12 chars part + 4 chars kanban + variable serial
            if (remaining.Length >= 5)
            {
                result.KanbanCode = remaining.Substring(0, 4).Trim();
                result.SerialNumber = remaining.Substring(4).Trim();
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = $"Cannot parse Kanban Code and Serial from: '{remaining}'";
                return result;
            }
        }

        // Validate we have all required parts
        if (string.IsNullOrWhiteSpace(result.PartNumber) ||
            string.IsNullOrWhiteSpace(result.KanbanCode) ||
            string.IsNullOrWhiteSpace(result.SerialNumber))
        {
            result.IsValid = false;
            result.ErrorMessage = $"Missing required components. Part: '{result.PartNumber}', Kanban: '{result.KanbanCode}', Serial: '{result.SerialNumber}'";
            return result;
        }

        result.IsValid = true;
        _logger.LogDebug("Parsed Internal Kanban: Part={PartNumber}, Kanban={KanbanCode}, Serial={SerialNumber}",
            result.PartNumber, result.KanbanCode, result.SerialNumber);

        return result;
    }

    /// <summary>
    /// Issue #2: Normalize part number by removing dashes for comparison
    /// "62730-08201-00" → "627300820100"
    /// </summary>
    private static string NormalizePartNumber(string? partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return string.Empty;
        return partNumber.Replace("-", "").Trim();
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

            // GAP-015: TOYOTA VALIDATION - Validate palletization code matching
            // Get the planned item to check its palletization code
            var plannedItem = await _skidBuildRepository.GetPlannedItemByIdAsync(request.PlannedItemId);
            if (plannedItem != null && !string.IsNullOrWhiteSpace(plannedItem.PalletizationCode)
                && !string.IsNullOrWhiteSpace(request.PalletizationCode))
            {
                var palletizationValidation = _toyotaValidationService.ValidatePalletizationCode(
                    request.PalletizationCode, // Manifest palletization (from scan)
                    plannedItem.PalletizationCode); // Kanban palletization (from planned item)

                if (!palletizationValidation.IsValid)
                {
                    return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                        "Palletization Code Mismatch",
                        palletizationValidation.ErrorMessage ?? "Palletization code validation failed");
                }
            }

            // ===== ISSUE #2 & #4: INTERNAL KANBAN PARSING AND VALIDATION =====
            ParsedInternalKanban? parsedKanban = null;

            if (!string.IsNullOrWhiteSpace(request.InternalKanban))
            {
                // Issue #2: Parse Internal Kanban using fixed-position format
                parsedKanban = ParseInternalKanban(request.InternalKanban);

                if (!parsedKanban.IsValid)
                {
                    _logger.LogWarning("Invalid Internal Kanban format: {Error}", parsedKanban.ErrorMessage);
                    return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                        "Invalid Internal Kanban Format",
                        parsedKanban.ErrorMessage ?? "Failed to parse Internal Kanban");
                }

                // Issue #2: Validate Part Number matches PlannedItem
                // NormalizePartNumber removes dashes so both formats match:
                //   - "62730-08201-00" → "627300820100"
                //   - "627300820100"   → "627300820100" (unchanged)
                if (plannedItem != null)
                {
                    var normalizedPlannedPart = NormalizePartNumber(plannedItem.PartNumber);
                    var normalizedScannedPart = NormalizePartNumber(parsedKanban.PartNumber);

                    if (!string.Equals(normalizedPlannedPart, normalizedScannedPart, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(
                            "Part Number mismatch: PlannedItem='{PlannedPart}' ({NormalizedPlanned}), InternalKanban='{ScannedPart}' ({NormalizedScanned})",
                            plannedItem.PartNumber, normalizedPlannedPart,
                            parsedKanban.PartNumber, normalizedScannedPart);

                        return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                            "Part Number Mismatch",
                            $"Internal Kanban Part '{parsedKanban.PartNumber}' does not match Toyota Kanban Part '{plannedItem.PartNumber}'");
                    }

                    // Issue #2: Validate Kanban Code matches PlannedItem
                    if (!string.IsNullOrWhiteSpace(plannedItem.KanbanNumber))
                    {
                        if (!string.Equals(plannedItem.KanbanNumber.Trim(), parsedKanban.KanbanCode.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning(
                                "Kanban Code mismatch: PlannedItem='{PlannedKanban}', InternalKanban='{ScannedKanban}'",
                                plannedItem.KanbanNumber, parsedKanban.KanbanCode);

                            return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                                "Kanban Code Mismatch",
                                $"Internal Kanban Code '{parsedKanban.KanbanCode}' does not match Toyota Kanban Code '{plannedItem.KanbanNumber}'");
                        }
                    }

                    _logger.LogInformation(
                        "Internal Kanban validated: Part={PartNumber}, Kanban={KanbanCode}, Serial={SerialNumber}",
                        parsedKanban.PartNumber, parsedKanban.KanbanCode, parsedKanban.SerialNumber);
                }

                // Issue #4: Time-based Serial Number duplicate check (uses KanbanDuplicateWindowHours)
                if (session.OrderId.HasValue)
                {
                    var siteSettings = await _siteSettingsRepository.GetAsync();
                    bool allowDuplicates = siteSettings?.KanbanAllowDuplicates ?? false;
                    int windowHours = siteSettings?.KanbanDuplicateWindowHours ?? 24;

                    if (!allowDuplicates)
                    {
                        // Issue #4: Check if Serial Number was scanned within time window (across ALL orders)
                        var isSerialDuplicate = await _skidBuildRepository.IsSerialNumberScannedWithinWindowAsync(
                            parsedKanban.SerialNumber,
                            windowHours);

                        if (isSerialDuplicate)
                        {
                            _logger.LogWarning(
                                "Duplicate Serial Number blocked: '{SerialNumber}' scanned within last {WindowHours} hours",
                                parsedKanban.SerialNumber,
                                windowHours);

                            return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                                "Duplicate Serial Number",
                                $"Serial '{parsedKanban.SerialNumber}' was already scanned within the last {windowHours} hours");
                        }
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Duplicate kanban allowed (setting enabled): Serial='{SerialNumber}'",
                            parsedKanban.SerialNumber);
                    }
                }
            }

            // STRICT DUPLICATE CHECK: Validate Toyota Kanban duplicates (ALWAYS enforced - no setting)
            if (session.OrderId.HasValue)
            {
                var isToyotaKanbanDuplicate = await _skidBuildRepository.IsToyotaKanbanAlreadyScannedAsync(
                    session.OrderId.Value,
                    request.PlannedItemId,
                    request.BoxNumber);

                if (isToyotaKanbanDuplicate)
                {
                    _logger.LogWarning(
                        "Duplicate Toyota Kanban blocked: PlannedItemId '{PlannedItemId}', BoxNumber '{BoxNumber}' for Order: {OrderId}",
                        request.PlannedItemId,
                        request.BoxNumber,
                        session.OrderId.Value);

                    return ApiResponse<SkidBuildScanResponseDto>.ErrorResponse(
                        "Duplicate Toyota Kanban",
                        $"Toyota Kanban for this part and box number has already been scanned for this order");
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
                InternalKanbanSerial = parsedKanban?.SerialNumber, // Issue #4: Store parsed serial for duplicate checking
                PalletizationCode = request.PalletizationCode,
                ScannedAt = DateTime.UtcNow,
                ScannedBy = resolvedUserId,
                CreatedAt = DateTime.UtcNow
            };

            var createdScan = await _skidBuildRepository.CreateScanAsync(scan);

            // Update order status to SkidBuilding if this is the first scan
            if (session.OrderId.HasValue)
            {
                var order = await _skidBuildRepository.GetOrderByIdAsync(session.OrderId.Value);
                if (order != null && order.Status == OrderStatus.Planned)
                {
                    order.Status = OrderStatus.SkidBuilding;
                    order.UpdatedAt = DateTime.UtcNow;
                    await _skidBuildRepository.UpdateOrderAsync(order);

                    _logger.LogInformation("Order {OrderId} status updated to SkidBuilding after first scan", order.OrderId);
                }
            }

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
                    RfidDetails = new List<ToyotaRfidDetail> { new ToyotaRfidDetail { Rfid = "", Type = "" } } // GAP-001: Toyota spec requires empty array with empty object, not null
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
                order.Status = OrderStatus.SkidBuilt;
                order.ToyotaSkidBuildConfirmationNumber = toyotaResponse.ConfirmationNumber;
                order.ToyotaSkidBuildStatus = "confirmed";
                order.ToyotaSkidBuildSubmittedAt = DateTime.UtcNow;
                order.ToyotaSkidBuildErrorMessage = null;

                _logger.LogInformation("Toyota API submission successful - ConfirmationNumber: {ConfirmationNumber}",
                    toyotaResponse.ConfirmationNumber);
            }
            else
            {
                order.Status = OrderStatus.SkidBuildError;
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

    /// <summary>
    /// Restart a skid build session - clears all scans and resets order to Planned status
    /// BLOCKS if order already confirmed by Toyota
    /// </summary>
    public async Task<ApiResponse<RestartSessionResponseDto>> RestartSessionAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("[SKID BUILD RESTART] Starting restart for session: {SessionId}", sessionId);

            // 1. Get session by sessionId
            var session = await _skidBuildRepository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                    "Session not found",
                    $"Session {sessionId} does not exist");
            }

            // 2. Validate session has an OrderId
            if (session.OrderId == null)
            {
                return ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                    "Invalid session",
                    "Session does not have an associated order");
            }

            // 3. Get Order from session.OrderId
            var order = await _skidBuildRepository.GetOrderByIdAsync(session.OrderId.Value);

            if (order == null)
            {
                return ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                    "Order not found",
                    $"Order {session.OrderId} does not exist");
            }

            // 4. BLOCK if Toyota confirmed
            if (order.ToyotaSkidBuildStatus == "confirmed")
            {
                _logger.LogWarning("[SKID BUILD RESTART] Blocked - Order {OrderNumber} already confirmed by Toyota (Confirmation: {ConfirmationNumber})",
                    order.RealOrderNumber, order.ToyotaSkidBuildConfirmationNumber);

                return ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                    "Cannot restart - already confirmed by Toyota",
                    $"Order {order.RealOrderNumber} has been confirmed by Toyota (Confirmation: {order.ToyotaSkidBuildConfirmationNumber}). Restart is not allowed.");
            }

            _logger.LogInformation("[SKID BUILD RESTART] Proceeding with restart - Order: {OrderNumber}, Status: {ToyotaStatus}",
                order.RealOrderNumber, order.ToyotaSkidBuildStatus ?? "none");

            // 5. Get all PlannedItemIds for this Order
            var plannedItemIds = order.PlannedItems.Select(pi => pi.PlannedItemId).ToList();

            // 6. Delete all SkidScans where PlannedItemId in those IDs
            int scansDeleted = await _skidBuildRepository.DeleteSkidScansByPlannedItemIdsAsync(plannedItemIds);
            _logger.LogInformation("[SKID BUILD RESTART] Deleted {Count} SkidScans for order {OrderNumber}",
                scansDeleted, order.RealOrderNumber);

            // 7. Delete all SkidBuildExceptions where OrderId = order.OrderId
            int exceptionsDeleted = await _skidBuildRepository.DeleteExceptionsByOrderIdAsync(order.OrderId);
            _logger.LogInformation("[SKID BUILD RESTART] Deleted {Count} SkidBuildExceptions for order {OrderNumber}",
                exceptionsDeleted, order.RealOrderNumber);

            // 8. Reset Order
            order.Status = OrderStatus.Planned;
            order.ToyotaSkidBuildConfirmationNumber = null;
            order.ToyotaSkidBuildStatus = null;
            order.ToyotaSkidBuildErrorMessage = null;
            order.ToyotaSkidBuildSubmittedAt = null;

            await _skidBuildRepository.UpdateOrderAsync(order);
            _logger.LogInformation("[SKID BUILD RESTART] Order {OrderNumber} reset to Planned status", order.RealOrderNumber);

            // 9. Mark session as "cancelled"
            session.Status = "cancelled";
            session.CompletedAt = DateTime.UtcNow;
            await _skidBuildRepository.UpdateSessionAsync(session);
            _logger.LogInformation("[SKID BUILD RESTART] Session {SessionId} marked as cancelled", sessionId);

            // 10. Return success response
            var responseDto = new RestartSessionResponseDto
            {
                Success = true,
                Message = $"Order {order.RealOrderNumber} has been reset. All scans and exceptions cleared.",
                NewSessionId = null
            };

            return ApiResponse<RestartSessionResponseDto>.SuccessResponse(
                responseDto,
                $"Session restarted successfully. Order reset to Planned status.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SKID BUILD RESTART] Error restarting session: {SessionId}", sessionId);
            return ApiResponse<RestartSessionResponseDto>.ErrorResponse(
                "Failed to restart session",
                ex.Message);
        }
    }
}
