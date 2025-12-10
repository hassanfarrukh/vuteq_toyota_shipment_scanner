// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPreShipmentExceptions - Exceptions during pre-shipment scan

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Pre-Shipment Exception entity for tracking exceptions during pre-shipment
/// </summary>
[Table("tblPreShipmentExceptions")]
public class PreShipmentException : AuditableEntity
{
    [Key]
    public Guid ExceptionId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string ShipmentId { get; set; } = null!;

    [MaxLength(200)]
    public string? ExceptionType { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }

    [MaxLength(50)]
    public string? RelatedSkidId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ShipmentId))]
    public virtual PreShipmentShipment Shipment { get; set; } = null!;
}
