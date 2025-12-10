// Author: Hassan
// Date: 2025-12-04
// Description: Entity representing tblPlannedItems - Expected parts for orders with manifest tracking

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Planned Item entity representing expected parts for orders
/// Includes manifest number for skid tracking and short/over quantities
/// </summary>
[Table("tblPlannedItems")]
public class PlannedItem : AuditableEntity
{
    [Key]
    public Guid PlannedItemId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to Order (GUID)
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Part number (e.g., "68101-0E120-00")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Quantity per container (QPC) - formerly LotQty
    /// </summary>
    public int? Qpc { get; set; }

    /// <summary>
    /// Kanban number (e.g., "FA99", "TF63")
    /// </summary>
    [MaxLength(50)]
    public string? KanbanNumber { get; set; }

    /// <summary>
    /// Total boxes planned - formerly LotOrdered
    /// </summary>
    public int? TotalBoxPlanned { get; set; }

    /// <summary>
    /// Manifest number - identifies which skid this item belongs to (MANIFEST_NO)
    /// </summary>
    public long ManifestNo { get; set; }

    /// <summary>
    /// Remaining boxes to ship (SHORT/OVER)
    /// </summary>
    public int? ShortOver { get; set; }

    /// <summary>
    /// Total pieces count (PIECES)
    /// </summary>
    public int? Pieces { get; set; }

    /// <summary>
    /// Palletization code (e.g., "CA", "U8", "IA")
    /// </summary>
    [MaxLength(20)]
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// TSCS external order ID (e.g., 48249852)
    /// </summary>
    public long ExternalOrderId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<SkidScan> SkidScans { get; set; } = new List<SkidScan>();
}
