// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPlannedSkids - Expected skids for routes

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Planned Skid entity representing expected skids for routes
/// </summary>
[Table("tblPlannedSkids")]
public class PlannedSkid : AuditableEntity
{
    [Key]
    public Guid PlannedSkidId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string SkidId { get; set; } = null!;

    [MaxLength(50)]
    public string? OrderNumber { get; set; }

    public int? PartCount { get; set; }

    [MaxLength(200)]
    public string? Destination { get; set; }

    [MaxLength(20)]
    public string? Plant { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }
}
