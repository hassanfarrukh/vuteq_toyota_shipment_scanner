// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPreShipmentShipments - Pre-shipment scan workflow

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Pre-Shipment Shipment entity for pre-shipment scan workflow
/// </summary>
[Table("tblPreShipmentShipments")]
public class PreShipmentShipment : AuditableEntity
{
    [Key]
    [MaxLength(50)]
    public string ShipmentId { get; set; } = null!; // Format: SHP{timestamp}

    [Required]
    public Guid CreatedByUserId { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "in-progress";

    public int CurrentScreen { get; set; } = 1;

    [MaxLength(50)]
    public string? TrailerNumber { get; set; }

    [MaxLength(50)]
    public string? SealNumber { get; set; }

    [MaxLength(200)]
    public string? CarrierName { get; set; }

    [MaxLength(200)]
    public string? DriverName { get; set; }

    public string? Notes { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual UserMaster CreatedByUser { get; set; } = null!;

    public virtual ICollection<PreShipmentManifest> PreShipmentManifests { get; set; } = new List<PreShipmentManifest>();
    public virtual ICollection<PreShipmentScannedSkid> PreShipmentScannedSkids { get; set; } = new List<PreShipmentScannedSkid>();
    public virtual ICollection<PreShipmentException> PreShipmentExceptions { get; set; } = new List<PreShipmentException>();
}
