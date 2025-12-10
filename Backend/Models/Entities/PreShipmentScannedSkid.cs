// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPreShipmentScannedSkids - Skids scanned during pre-shipment

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Pre-Shipment Scanned Skid entity for skids scanned during pre-shipment
/// </summary>
[Table("tblPreShipmentScannedSkids")]
public class PreShipmentScannedSkid : AuditableEntity
{
    [Key]
    public Guid ScannedSkidId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string ShipmentId { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string SkidId { get; set; } = null!;

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    public int? PartCount { get; set; }

    [MaxLength(200)]
    public string? Destination { get; set; }

    [MaxLength(500)]
    public string? ScannedValue { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ShipmentId))]
    public virtual PreShipmentShipment Shipment { get; set; } = null!;
}
