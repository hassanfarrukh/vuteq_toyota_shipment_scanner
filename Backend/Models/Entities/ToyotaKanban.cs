// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblToyotaKanbans - 200+ character QR codes with 24 fields

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Toyota Kanban entity for 200+ character QR codes with 24 fields
/// </summary>
[Table("tblToyotaKanbans")]
public class ToyotaKanban : AuditableEntity
{
    [Key]
    public Guid KanbanId { get; set; } = Guid.NewGuid();

    [Required]
    public string QrCode { get; set; } = null!;

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    [MaxLength(10)]
    public string? DockCode { get; set; }

    [MaxLength(20)]
    public string? Quantity { get; set; }

    [MaxLength(100)]
    public string? KanbanNumber { get; set; }

    [MaxLength(200)]
    public string? ShipToAddress1 { get; set; }

    [MaxLength(200)]
    public string? ShipToAddress2 { get; set; }

    [MaxLength(50)]
    public string? DeliveryDate { get; set; }

    [MaxLength(50)]
    public string? DeliveryTime { get; set; }

    [MaxLength(20)]
    public string? PlantCode { get; set; }

    [MaxLength(50)]
    public string? Route { get; set; }

    [MaxLength(50)]
    public string? ContainerType { get; set; }

    [MaxLength(20)]
    public string? PalletCode { get; set; }

    [MaxLength(50)]
    public string? StorageLocation { get; set; }

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    [MaxLength(20)]
    public string? SequenceNumber { get; set; }

    [MaxLength(50)]
    public string? BatchNumber { get; set; }

    [MaxLength(50)]
    public string? ManufacturingDate { get; set; }

    [MaxLength(50)]
    public string? ExpiryDate { get; set; }

    [MaxLength(50)]
    public string? LotNumber { get; set; }

    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    [MaxLength(20)]
    public string? RevisionLevel { get; set; }

    [MaxLength(50)]
    public string? QualityStatus { get; set; }

    public Guid? SessionId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? ScannedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession? Session { get; set; }

    public virtual ICollection<InternalKanban> InternalKanbans { get; set; } = new List<InternalKanban>();
}
