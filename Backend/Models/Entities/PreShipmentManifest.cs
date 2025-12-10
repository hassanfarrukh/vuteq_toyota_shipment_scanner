// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPreShipmentManifests - Manifests scanned during pre-shipment

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Pre-Shipment Manifest entity for manifests scanned during pre-shipment
/// </summary>
[Table("tblPreShipmentManifests")]
public class PreShipmentManifest : AuditableEntity
{
    [Key]
    public Guid ManifestRecordId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string ShipmentId { get; set; } = null!;

    [MaxLength(20)]
    public string? ManifestId { get; set; } // Last 8 chars

    [MaxLength(500)]
    public string? ScannedValue { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ShipmentId))]
    public virtual PreShipmentShipment Shipment { get; set; } = null!;
}
