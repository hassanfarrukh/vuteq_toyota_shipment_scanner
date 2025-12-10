// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblDockOrderStatusHistory - History of dock order status changes

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Dock Order Status History entity for tracking status changes
/// </summary>
[Table("tblDockOrderStatusHistory")]
public class DockOrderStatusHistory : AuditableEntity
{
    [Key]
    public Guid HistoryId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid DockOrderId { get; set; }

    [MaxLength(50)]
    public string? OldStatus { get; set; }

    [MaxLength(50)]
    public string? NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? ChangedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DockOrderId))]
    public virtual DockOrder DockOrder { get; set; } = null!;
}
