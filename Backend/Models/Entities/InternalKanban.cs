// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblInternalKanbans - PART/KANBAN/SERIAL format scans

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Internal Kanban entity for PART/KANBAN/SERIAL format scans
/// </summary>
[Table("tblInternalKanbans")]
public class InternalKanban : AuditableEntity
{
    [Key]
    public Guid InternalKanbanId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string ScanValue { get; set; } = null!;

    [MaxLength(100)]
    public string? ToyotaKanban { get; set; }

    [MaxLength(100)]
    public string? InternalKanbanValue { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public Guid? SessionId { get; set; }

    public Guid? ToyotaKanbanId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? ScannedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession? Session { get; set; }

    [ForeignKey(nameof(ToyotaKanbanId))]
    public virtual ToyotaKanban? ToyotaKanbanReference { get; set; }
}
