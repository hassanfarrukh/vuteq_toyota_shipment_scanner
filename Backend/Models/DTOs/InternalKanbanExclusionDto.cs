// Author: Hassan
// Date: 2025-01-13
// Description: DTOs for Internal Kanban Exclusion management

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// DTO for Internal Kanban Exclusion response
/// </summary>
public class InternalKanbanExclusionDto
{
    /// <summary>
    /// Unique identifier for the exclusion
    /// </summary>
    public Guid ExclusionId { get; set; }

    /// <summary>
    /// Toyota part number
    /// </summary>
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Flag indicating if part is excluded from validation
    /// </summary>
    public bool IsExcluded { get; set; }

    /// <summary>
    /// Mode of entry - 'single' or 'bulk'
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// User ID who created this exclusion
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Username of user who created this exclusion
    /// </summary>
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who last updated this exclusion
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Username of user who last updated this exclusion
    /// </summary>
    public string? UpdatedByUsername { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a single Internal Kanban Exclusion
/// Frontend only sends PartNumber and InternalKanbanExclusion
/// </summary>
public class CreateInternalKanbanExclusionDto
{
    /// <summary>
    /// Toyota part number - Required
    /// </summary>
    [Required(ErrorMessage = "Part number is required")]
    [MaxLength(100, ErrorMessage = "Part number cannot exceed 100 characters")]
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Flag indicating if part should be excluded from validation - Required
    /// True = skip validation, False = enforce validation
    /// </summary>
    [Required(ErrorMessage = "IsExcluded flag is required")]
    public bool IsExcluded { get; set; }
}

/// <summary>
/// Request DTO for updating an Internal Kanban Exclusion
/// </summary>
public class UpdateInternalKanbanExclusionDto
{
    /// <summary>
    /// Toyota part number - Required
    /// </summary>
    [Required(ErrorMessage = "Part number is required")]
    [MaxLength(100, ErrorMessage = "Part number cannot exceed 100 characters")]
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Flag indicating if part should be excluded from validation - Required
    /// True = skip validation, False = enforce validation
    /// </summary>
    [Required(ErrorMessage = "IsExcluded flag is required")]
    public bool IsExcluded { get; set; }
}

/// <summary>
/// Response DTO for bulk upload results
/// </summary>
public class BulkUploadResultDto
{
    /// <summary>
    /// Total number of records processed
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Number of successfully created records
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed records
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// List of error messages for failed records
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// List of successfully created exclusions
    /// </summary>
    public List<InternalKanbanExclusionDto> CreatedExclusions { get; set; } = new List<InternalKanbanExclusionDto>();
}
