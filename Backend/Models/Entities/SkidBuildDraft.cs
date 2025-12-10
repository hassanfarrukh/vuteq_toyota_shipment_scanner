// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblSkidBuildDrafts - Draft saves for skid build sessions

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Skid Build Draft entity for saving draft progress
/// </summary>
[Table("tblSkidBuildDrafts")]
public class SkidBuildDraft : AuditableEntity
{
    [Key]
    public Guid DraftId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string? OwkNumber { get; set; }

    public string? DraftData { get; set; }

    public int? CurrentScreen { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession Session { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;
}
