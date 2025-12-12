// Author: Hassan
// Date: 2025-12-04
// Description: Entity representing tblOrderUploads - Order file upload tracking

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Order Upload entity for tracking uploaded order files
/// </summary>
[Table("tblOrderUploads")]
public class OrderUpload : AuditableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = null!;

    [Required]
    public long FileSize { get; set; }

    [MaxLength(1000)]
    public string? FilePath { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public Guid? UploadedBy { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of orders created from this upload
    /// </summary>
    public int OrdersCreated { get; set; }

    /// <summary>
    /// Total number of items created from this upload
    /// </summary>
    public int TotalItemsCreated { get; set; }

    /// <summary>
    /// Total number of unique manifests in this upload
    /// </summary>
    public int TotalManifestsCreated { get; set; }

    /// <summary>
    /// Supplier code from NAMC Detail sheet
    /// </summary>
    public int? SupplierCode { get; set; }

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    [MaxLength(20)]
    public string? PlantCode { get; set; }

    /// <summary>
    /// Total planned count from NAMC Summary
    /// </summary>
    public int? TotalPlanned { get; set; }

    /// <summary>
    /// Total shipped count from NAMC Summary
    /// </summary>
    public int? TotalShipped { get; set; }

    /// <summary>
    /// Total shorted count from NAMC Summary
    /// </summary>
    public int? TotalShorted { get; set; }

    /// <summary>
    /// Total late count from NAMC Summary
    /// </summary>
    public int? TotalLate { get; set; }

    /// <summary>
    /// Total pending count from NAMC Summary
    /// </summary>
    public int? TotalPending { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UploadedBy))]
    public virtual UserMaster? UploadedByUser { get; set; }
}
