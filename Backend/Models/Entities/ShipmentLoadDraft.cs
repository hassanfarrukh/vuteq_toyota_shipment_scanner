// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblShipmentLoadDrafts - Draft saves for shipment load sessions

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Shipment Load Draft entity for saving draft progress
/// </summary>
[Table("tblShipmentLoadDrafts")]
public class ShipmentLoadDraft : AuditableEntity
{
    [Key]
    public Guid DraftId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string? RouteNumber { get; set; }

    public string? DraftData { get; set; }

    public int? CurrentScreen { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual ShipmentLoadSession Session { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;
}
