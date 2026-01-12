// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblScannedSkids - Skids scanned during shipment load

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Scanned Skid entity for skids scanned during shipment loading
/// </summary>
[Table("tblScannedSkids")]
public class ScannedSkid : AuditableEntity
{
    [Key]
    public Guid ScannedSkidId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SkidId { get; set; } = null!;

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    public int? PartCount { get; set; }

    [MaxLength(200)]
    public string? Destination { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual ShipmentLoadSession Session { get; set; } = null!;
}
