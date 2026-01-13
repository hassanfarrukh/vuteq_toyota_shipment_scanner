// Author: Hassan
// Date: 2025-01-13
// Description: Service for Internal Kanban Exclusion management - handles business logic and Excel parsing

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;
using ClosedXML.Excel;

namespace Backend.Services;

/// <summary>
/// Interface for Internal Kanban Exclusion service operations
/// </summary>
public interface IInternalKanbanExclusionService
{
    Task<ApiResponse<IEnumerable<InternalKanbanExclusionDto>>> GetAllAsync();
    Task<ApiResponse<InternalKanbanExclusionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<InternalKanbanExclusionDto>> CreateAsync(CreateInternalKanbanExclusionDto request, Guid userId);
    Task<ApiResponse<BulkUploadResultDto>> BulkUploadAsync(IFormFile file, Guid userId);
    Task<ApiResponse<InternalKanbanExclusionDto>> UpdateAsync(Guid id, UpdateInternalKanbanExclusionDto request, Guid userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}

/// <summary>
/// Service implementation for Internal Kanban Exclusion management
/// </summary>
public class InternalKanbanExclusionService : IInternalKanbanExclusionService
{
    private readonly IInternalKanbanExclusionRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<InternalKanbanExclusionService> _logger;

    public InternalKanbanExclusionService(
        IInternalKanbanExclusionRepository repository,
        IUserRepository userRepository,
        ILogger<InternalKanbanExclusionService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all exclusions
    /// </summary>
    public async Task<ApiResponse<IEnumerable<InternalKanbanExclusionDto>>> GetAllAsync()
    {
        try
        {
            var exclusions = await _repository.GetAllAsync();
            var dtos = new List<InternalKanbanExclusionDto>();

            foreach (var exclusion in exclusions)
            {
                dtos.Add(await MapToDtoAsync(exclusion));
            }

            return ApiResponse<IEnumerable<InternalKanbanExclusionDto>>.SuccessResponse(
                dtos,
                "Internal kanban exclusions retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban exclusions");
            return ApiResponse<IEnumerable<InternalKanbanExclusionDto>>.ErrorResponse(
                "Failed to retrieve internal kanban exclusions",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get exclusion by ID
    /// </summary>
    public async Task<ApiResponse<InternalKanbanExclusionDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var exclusion = await _repository.GetByIdAsync(id);
            if (exclusion == null)
            {
                return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                    "Exclusion not found",
                    $"Internal kanban exclusion with ID {id} does not exist"
                );
            }

            return ApiResponse<InternalKanbanExclusionDto>.SuccessResponse(
                await MapToDtoAsync(exclusion),
                "Internal kanban exclusion retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving internal kanban exclusion: {ExclusionId}", id);
            return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                "Failed to retrieve internal kanban exclusion",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Create a single exclusion (Mode = 'single')
    /// </summary>
    public async Task<ApiResponse<InternalKanbanExclusionDto>> CreateAsync(
        CreateInternalKanbanExclusionDto request,
        Guid userId)
    {
        try
        {
            // Check if part number already exists
            var existing = await _repository.GetByPartNumberAsync(request.PartNumber);
            if (existing != null)
            {
                return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                    "Exclusion creation failed",
                    $"Part number '{request.PartNumber}' already exists in exclusions"
                );
            }

            var now = DateTime.Now;
            var exclusion = new InternalKanbanExclusion
            {
                ExclusionId = Guid.NewGuid(),
                PartNumber = request.PartNumber,
                IsExcluded = request.IsExcluded,
                Mode = "single", // Auto-set by controller
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedBy = userId,  // Set UpdatedBy same as CreatedBy on creation
                UpdatedAt = now      // Set UpdatedAt same as CreatedAt on creation
            };

            var created = await _repository.CreateAsync(exclusion);

            _logger.LogInformation("Internal kanban exclusion created: {PartNumber} by user {UserId}",
                created.PartNumber, userId);

            return ApiResponse<InternalKanbanExclusionDto>.SuccessResponse(
                await MapToDtoAsync(created),
                "Internal kanban exclusion created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating internal kanban exclusion: {PartNumber}", request.PartNumber);
            return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                "Failed to create internal kanban exclusion",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Bulk upload exclusions from Excel file (Mode = 'bulk')
    /// Excel format: Single sheet with single column "PartNumber"
    /// </summary>
    public async Task<ApiResponse<BulkUploadResultDto>> BulkUploadAsync(IFormFile file, Guid userId)
    {
        var result = new BulkUploadResultDto();

        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return ApiResponse<BulkUploadResultDto>.ErrorResponse(
                    "Invalid file",
                    "Please provide a valid Excel file"
                );
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<BulkUploadResultDto>.ErrorResponse(
                    "Invalid file format",
                    "Only Excel files (.xlsx, .xls) are supported"
                );
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // First sheet

            // Find the header row (should contain "PartNumber")
            var headerRow = worksheet.Row(1);
            var partNumberColumn = -1;

            // Find PartNumber column
            for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
            {
                var cellValue = headerRow.Cell(col).GetString().Trim();
                if (cellValue.Equals("PartNumber", StringComparison.OrdinalIgnoreCase))
                {
                    partNumberColumn = col;
                    break;
                }
            }

            if (partNumberColumn == -1)
            {
                return ApiResponse<BulkUploadResultDto>.ErrorResponse(
                    "Invalid Excel format",
                    "Excel file must contain a 'PartNumber' column header"
                );
            }

            var exclusionsToCreate = new List<InternalKanbanExclusion>();
            var processedPartNumbers = new HashSet<string>();

            // Process rows (starting from row 2, skipping header)
            var rows = worksheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                result.TotalProcessed++;
                var rowNumber = row.RowNumber();

                try
                {
                    var partNumber = row.Cell(partNumberColumn).GetString().Trim();

                    // Validate part number
                    if (string.IsNullOrWhiteSpace(partNumber))
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Row {rowNumber}: Part number is empty");
                        continue;
                    }

                    if (partNumber.Length > 100)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Row {rowNumber}: Part number '{partNumber}' exceeds 100 characters");
                        continue;
                    }

                    // Check for duplicates within the file
                    if (processedPartNumbers.Contains(partNumber))
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Row {rowNumber}: Duplicate part number '{partNumber}' in file");
                        continue;
                    }

                    // Check if already exists in database
                    var existing = await _repository.GetByPartNumberAsync(partNumber);
                    if (existing != null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Row {rowNumber}: Part number '{partNumber}' already exists in database");
                        continue;
                    }

                    // Add to creation list
                    var now = DateTime.Now;
                    exclusionsToCreate.Add(new InternalKanbanExclusion
                    {
                        ExclusionId = Guid.NewGuid(),
                        PartNumber = partNumber,
                        IsExcluded = true, // Default to true for bulk uploads
                        Mode = "bulk", // Auto-set by controller
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedBy = userId,  // Set UpdatedBy same as CreatedBy on creation
                        UpdatedAt = now      // Set UpdatedAt same as CreatedAt on creation
                    });

                    processedPartNumbers.Add(partNumber);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Row {rowNumber}: Error processing row - {ex.Message}");
                }
            }

            // Bulk create valid exclusions
            if (exclusionsToCreate.Count > 0)
            {
                var created = await _repository.CreateBulkAsync(exclusionsToCreate);
                result.SuccessCount = created.Count;

                // Map to DTOs with usernames
                foreach (var exclusion in created)
                {
                    result.CreatedExclusions.Add(await MapToDtoAsync(exclusion));
                }

                _logger.LogInformation("Bulk upload: {SuccessCount} exclusions created, {FailedCount} failed by user {UserId}",
                    result.SuccessCount, result.FailedCount, userId);
            }

            return ApiResponse<BulkUploadResultDto>.SuccessResponse(
                result,
                $"Bulk upload completed: {result.SuccessCount} created, {result.FailedCount} failed"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk upload");
            return ApiResponse<BulkUploadResultDto>.ErrorResponse(
                "Failed to process bulk upload",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update an existing exclusion
    /// </summary>
    public async Task<ApiResponse<InternalKanbanExclusionDto>> UpdateAsync(
        Guid id,
        UpdateInternalKanbanExclusionDto request,
        Guid userId)
    {
        try
        {
            var exclusion = await _repository.GetByIdAsync(id);
            if (exclusion == null)
            {
                return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                    "Exclusion update failed",
                    $"Internal kanban exclusion with ID {id} does not exist"
                );
            }

            // Check if new part number conflicts with existing records (excluding current)
            var existing = await _repository.GetByPartNumberAsync(request.PartNumber);
            if (existing != null && existing.ExclusionId != id)
            {
                return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                    "Exclusion update failed",
                    $"Part number '{request.PartNumber}' already exists in another exclusion"
                );
            }

            // Update properties
            exclusion.PartNumber = request.PartNumber;
            exclusion.IsExcluded = request.IsExcluded;
            exclusion.UpdatedBy = userId;
            exclusion.UpdatedAt = DateTime.Now;

            var updated = await _repository.UpdateAsync(exclusion);

            _logger.LogInformation("Internal kanban exclusion updated: {ExclusionId} by user {UserId}",
                id, userId);

            return ApiResponse<InternalKanbanExclusionDto>.SuccessResponse(
                await MapToDtoAsync(updated),
                "Internal kanban exclusion updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating internal kanban exclusion: {ExclusionId}", id);
            return ApiResponse<InternalKanbanExclusionDto>.ErrorResponse(
                "Failed to update internal kanban exclusion",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Delete an exclusion
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var exclusion = await _repository.GetByIdAsync(id);
            if (exclusion == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Exclusion deletion failed",
                    $"Internal kanban exclusion with ID {id} does not exist"
                );
            }

            var deleted = await _repository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Internal kanban exclusion deleted: {ExclusionId}", id);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Internal kanban exclusion deleted successfully"
                );
            }

            return ApiResponse<bool>.ErrorResponse(
                "Exclusion deletion failed",
                "Failed to delete internal kanban exclusion"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting internal kanban exclusion: {ExclusionId}", id);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete internal kanban exclusion",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Map InternalKanbanExclusion entity to DTO with usernames
    /// </summary>
    private async Task<InternalKanbanExclusionDto> MapToDtoAsync(InternalKanbanExclusion exclusion)
    {
        // Fetch usernames from UserRepository
        string? createdByUsername = null;
        string? updatedByUsername = null;

        try
        {
            var createdByUser = await _userRepository.GetByIdAsync(exclusion.CreatedBy);
            createdByUsername = createdByUser?.Username;

            if (exclusion.UpdatedBy.HasValue)
            {
                var updatedByUser = await _userRepository.GetByIdAsync(exclusion.UpdatedBy.Value);
                updatedByUsername = updatedByUser?.Username;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching usernames for exclusion {ExclusionId}", exclusion.ExclusionId);
        }

        return new InternalKanbanExclusionDto
        {
            ExclusionId = exclusion.ExclusionId,
            PartNumber = exclusion.PartNumber,
            IsExcluded = exclusion.IsExcluded,
            Mode = exclusion.Mode,
            CreatedBy = exclusion.CreatedBy,
            CreatedByUsername = createdByUsername,
            CreatedAt = exclusion.CreatedAt,
            UpdatedBy = exclusion.UpdatedBy,
            UpdatedByUsername = updatedByUsername,
            UpdatedAt = exclusion.UpdatedAt
        };
    }
}
