// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblDockOrders - Dock monitor orders tracking

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Dock Order entity for dock monitor tracking
/// </summary>
[Table("tblDockOrders")]
public class DockOrder : AuditableEntity
{
    [Key]
    public Guid DockOrderId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = null!;

    [MaxLength(50)]
    public string? Route { get; set; }

    [MaxLength(200)]
    public string? Destination { get; set; }

    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(50)]
    public string? Location { get; set; }

    public DateTime? PlannedSkidBuild { get; set; }

    public DateTime? CompletedSkidBuild { get; set; }

    public DateTime? PlannedShipmentLoad { get; set; }

    public DateTime? CompletedShipmentLoad { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public bool IsSupplementOrder { get; set; } = false;

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }

    // Navigation properties
    public virtual ICollection<DockOrderStatusHistory> DockOrderStatusHistories { get; set; } = new List<DockOrderStatusHistory>();
}
