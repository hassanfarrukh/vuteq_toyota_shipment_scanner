// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblShipmentLoadSessions - Shipment loading workflow sessions

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Shipment Load Session entity for tracking shipment loading workflow
/// </summary>
[Table("tblShipmentLoadSessions")]
public class ShipmentLoadSession : AuditableEntity
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string? WarehouseId { get; set; }

    public string? Token { get; set; }

    [MaxLength(50)]
    public string? TrailerNumber { get; set; }

    [MaxLength(50)]
    public string? SealNumber { get; set; }

    [MaxLength(200)]
    public string? CarrierName { get; set; }

    [MaxLength(200)]
    public string? DriverName { get; set; }

    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "active";

    public int CurrentScreen { get; set; } = 1;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;

    public virtual ICollection<ScannedSkid> ScannedSkids { get; set; } = new List<ScannedSkid>();
    public virtual ICollection<ShipmentLoadException> ShipmentLoadExceptions { get; set; } = new List<ShipmentLoadException>();
    public virtual ICollection<ShipmentLoadDraft> ShipmentLoadDrafts { get; set; } = new List<ShipmentLoadDraft>();
}
