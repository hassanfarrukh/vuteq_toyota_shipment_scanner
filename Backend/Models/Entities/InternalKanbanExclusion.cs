// Author: Hassan
// Date: 2025-01-13
// Description: Entity for managing Toyota part numbers excluded from internal kanban validation

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Internal Kanban Exclusion entity for managing part numbers excluded from validation
/// </summary>
[Table("tblInternalKanbanExclusions")]
public class InternalKanbanExclusion
{
    /// <summary>
    /// Primary key - unique identifier for the exclusion
    /// </summary>
    [Key]
    public Guid ExclusionId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Toyota part number to exclude from internal kanban validation
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Flag indicating if this part should be excluded from internal kanban validation
    /// True = skip validation, False = enforce validation
    /// </summary>
    public bool IsExcluded { get; set; } = true;

    /// <summary>
    /// Mode of entry - 'single' for manual entry, 'bulk' for Excel upload
    /// Set by controller automatically, not from frontend
    /// </summary>
    [MaxLength(20)]
    public string? Mode { get; set; }

    /// <summary>
    /// User ID who created this exclusion
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this exclusion was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// User ID who last updated this exclusion
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when this exclusion was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
