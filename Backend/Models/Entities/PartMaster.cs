// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblPartMaster - Parts master data

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Part Master entity for parts catalog
/// </summary>
[Table("tblPartMaster")]
public class PartMaster : AuditableEntity
{
    [Key]
    public Guid PartId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string PartNo { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? WeightPerPiece { get; set; }

    [MaxLength(20)]
    public string? UomPerPiece { get; set; }

    [MaxLength(50)]
    public string? PartType { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? PackingStyle { get; set; }

    public bool CommonPart { get; set; } = false;

    public bool Discontinued { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }
}
