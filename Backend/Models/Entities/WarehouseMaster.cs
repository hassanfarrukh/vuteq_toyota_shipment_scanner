// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblWarehouseMaster - Warehouse locations master data

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Warehouse Master entity for warehouse locations
/// </summary>
[Table("tblWarehouseMaster")]
public class WarehouseMaster : AuditableEntity
{
    [Key]
    public Guid WarehouseId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? Zip { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? OfficeCode { get; set; }

    public bool IsActive { get; set; } = true;

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }

    // Navigation properties
    [ForeignKey(nameof(OfficeCode))]
    public virtual OfficeMaster? Office { get; set; }
}
