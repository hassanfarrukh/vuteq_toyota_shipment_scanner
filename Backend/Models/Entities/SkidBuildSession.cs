// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblSkidBuildSessions - Skid building workflow sessions

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Skid Build Session entity for tracking skid building workflow
/// </summary>
[Table("tblSkidBuildSessions")]
public class SkidBuildSession : AuditableEntity
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public string? WarehouseId { get; set; }

    /// <summary>
    /// Foreign key to Order (nullable - session can exist without order selected)
    /// </summary>
    public Guid? OrderId { get; set; }

    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    public string? Token { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "active";

    public int CurrentScreen { get; set; } = 1;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// DEPRECATED: Old confirmation number field (kept for backward compatibility)
    /// Use ToyotaConfirmationNumber and InternalReferenceNumber instead
    /// </summary>
    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Toyota API confirmation number (received from Toyota SCS API)
    /// NEW: Added for Toyota API integration
    /// </summary>
    [MaxLength(100)]
    public string? ToyotaConfirmationNumber { get; set; }

    /// <summary>
    /// Internal reference number generated before Toyota submission
    /// NEW: Used as placeholder until Toyota API integration is complete
    /// Format: SKB-{timestamp}-{random}
    /// </summary>
    [MaxLength(100)]
    public string? InternalReferenceNumber { get; set; }

    /// <summary>
    /// Toyota API submission status
    /// NEW: Track Toyota API submission lifecycle
    /// Values: "pending", "submitted", "confirmed", "error"
    /// </summary>
    [MaxLength(20)]
    public string? ToyotaSubmissionStatus { get; set; }

    /// <summary>
    /// Toyota API error message (if submission failed)
    /// NEW: Store error details from Toyota API
    /// </summary>
    [MaxLength(500)]
    public string? ToyotaErrorMessage { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public virtual ICollection<ToyotaManifest> ToyotaManifests { get; set; } = new List<ToyotaManifest>();
    public virtual ICollection<ToyotaKanban> ToyotaKanbans { get; set; } = new List<ToyotaKanban>();
    public virtual ICollection<InternalKanban> InternalKanbans { get; set; } = new List<InternalKanban>();
    public virtual ICollection<ScannedItem> ScannedItems { get; set; } = new List<ScannedItem>();
    public virtual ICollection<SkidBuildException> SkidBuildExceptions { get; set; } = new List<SkidBuildException>();
    public virtual ICollection<SkidBuildDraft> SkidBuildDrafts { get; set; } = new List<SkidBuildDraft>();
}
