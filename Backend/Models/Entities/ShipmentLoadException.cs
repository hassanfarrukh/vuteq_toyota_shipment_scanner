// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblShipmentLoadExceptions - Exceptions during shipment load

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Shipment Load Exception entity for tracking exceptions during shipment loading
/// </summary>
[Table("tblShipmentLoadExceptions")]
public class ShipmentLoadException : AuditableEntity
{
    [Key]
    public Guid ExceptionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [MaxLength(200)]
    public string? ExceptionType { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }

    [MaxLength(50)]
    public string? RelatedSkidId { get; set; }

    [MaxLength(50)]
    public string? CreatedByUser { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual ShipmentLoadSession Session { get; set; } = null!;
}
