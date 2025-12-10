// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblScannedItems - Items scanned during skid build

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Scanned Item entity for items scanned during skid build
/// </summary>
[Table("tblScannedItems")]
public class ScannedItem : AuditableEntity
{
    [Key]
    public Guid ScannedItemId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PartNumber { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? Quantity { get; set; }

    [MaxLength(100)]
    public string? KanbanNumber { get; set; }

    [MaxLength(100)]
    public string? InternalKanban { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession Session { get; set; } = null!;
}
