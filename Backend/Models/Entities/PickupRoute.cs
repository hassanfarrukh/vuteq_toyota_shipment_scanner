// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPickupRoutes - 50-character QR codes for pickup routes

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Pickup Route entity for 50-character QR codes
/// </summary>
[Table("tblPickupRoutes")]
public class PickupRoute : AuditableEntity
{
    [Key]
    public Guid RouteId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string QrCode { get; set; } = null!;

    [MaxLength(50)]
    public string? RouteNumber { get; set; }

    [MaxLength(20)]
    public string? Plant { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    [MaxLength(10)]
    public string? DockCode { get; set; }

    public int? EstimatedSkids { get; set; }

    [MaxLength(50)]
    public string? OrderDate { get; set; }

    [MaxLength(50)]
    public string? PickupDate { get; set; }

    [MaxLength(50)]
    public string? PickupTime { get; set; }
}
