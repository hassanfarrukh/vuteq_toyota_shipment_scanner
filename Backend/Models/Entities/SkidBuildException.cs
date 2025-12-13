// Author: Hassan
// Date: 2025-12-06
// Description: Entity representing tblSkidBuildExceptions - Tracks exceptions for Toyota API

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Skid Build Exception entity for tracking exceptions at order level for Toyota API
/// Exception codes: "10" (Revised Quantity), "11" (Modified QPC), "12" (Short Shipment), "20" (Non-Standard Packaging)
/// </summary>
[Table("tblSkidBuildExceptions")]
public class SkidBuildException : AuditableEntity
{
    [Key]
    public Guid ExceptionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Session ID (nullable - exceptions can exist without active session)
    /// Links the exception to a specific skid build session
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Skid number (NULL for order-level exceptions)
    /// </summary>
    public int? SkidNumber { get; set; }

    /// <summary>
    /// Exception code: "10", "11", "12", "20"
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ExceptionCode { get; set; } = null!;

    /// <summary>
    /// Optional comments (max 100 chars per Toyota API)
    /// </summary>
    [MaxLength(100)]
    public string? Comments { get; set; }

    /// <summary>
    /// User who created this exception (GUID reference)
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(SessionId))]
    public virtual SkidBuildSession? Session { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual UserMaster? CreatedByUser { get; set; }
}
