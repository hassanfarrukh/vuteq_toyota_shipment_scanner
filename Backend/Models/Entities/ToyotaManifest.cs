// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblToyotaManifests - 44-character QR codes for Toyota manifests

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Toyota Manifest entity for 44-character QR codes
/// </summary>
[Table("tblToyotaManifests")]
public class ToyotaManifest : AuditableEntity
{
    [Key]
    public Guid ManifestId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string QrCode { get; set; } = null!;

    [MaxLength(10)]
    public string? PlantPrefix { get; set; }

    [MaxLength(10)]
    public string? PlantCode { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    [MaxLength(10)]
    public string? DockCode { get; set; }

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    [MaxLength(50)]
    public string? LoadId { get; set; }

    [MaxLength(10)]
    public string? PalletizationCode { get; set; }

    [MaxLength(10)]
    public string? Mros { get; set; }

    [MaxLength(20)]
    public string? SkidId { get; set; }

    [MaxLength(20)]
    public string? FormattedSkidId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? ScannedBy { get; set; }

    public Guid? SessionId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession? Session { get; set; }
}
