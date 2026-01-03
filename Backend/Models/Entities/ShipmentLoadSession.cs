// Author: Hassan
// Date: 2025-12-17
// Description: Entity representing tblShipmentLoadSessions - Shipment loading workflow sessions
// Updated: Simplified for Toyota SCS integration - removed unused fields, added Toyota API response fields

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Shipment Load Session entity for tracking shipment loading workflow
/// Simplified structure for Toyota SCS Trailer API integration
/// </summary>
[Table("tblShipmentLoadSessions")]
public class ShipmentLoadSession : AuditableEntity
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Route code parsed from RouteNumber (all except last 2 chars)
    /// Example: "YUAN03" -> "YUAN"
    /// </summary>
    [MaxLength(2)]
    public string? Run { get; set; }

    [MaxLength(50)]
    public string? TrailerNumber { get; set; }

    [MaxLength(50)]
    public string? SealNumber { get; set; }

    /// <summary>
    /// Carrier SCAC code from driver badge (e.g., "RYDD")
    /// </summary>
    [MaxLength(6)]
    public string? LpCode { get; set; }

    /// <summary>
    /// Driver first name from driver badge (REQUIRED since dropHook=false)
    /// </summary>
    [MaxLength(9)]
    public string? DriverFirstName { get; set; }

    /// <summary>
    /// Driver last name from driver badge (REQUIRED since dropHook=false)
    /// </summary>
    [MaxLength(12)]
    public string? DriverLastName { get; set; }

    /// <summary>
    /// Supplier team first name (optional)
    /// </summary>
    [MaxLength(9)]
    public string? SupplierFirstName { get; set; }

    /// <summary>
    /// Supplier team last name (optional)
    /// </summary>
    [MaxLength(12)]
    public string? SupplierLastName { get; set; }

    /// <summary>
    /// Pickup date/time from checksheet scan (RFC3339 format for Toyota API)
    /// </summary>
    public DateTime? PickupDateTime { get; set; }

    /// <summary>
    /// ONE supplier code per transmission (5 digits)
    /// </summary>
    [MaxLength(5)]
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Session status: "active", "completed", "cancelled"
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// How this session was created: "PreShipment" or "ShipmentLoad"
    /// PreShipment = created from manifest scan before driver arrives
    /// ShipmentLoad = created from pickup QR when driver arrives (default)
    /// </summary>
    [MaxLength(20)]
    public string CreatedVia { get; set; } = "ShipmentLoad";

    public DateTime? CompletedAt { get; set; }

    // ===== TOYOTA API RESPONSE FIELDS =====
    /// <summary>
    /// Toyota API confirmation number from Shipment Load submission
    /// </summary>
    [MaxLength(100)]
    public string? ToyotaConfirmationNumber { get; set; }

    /// <summary>
    /// Toyota submission status: "pending", "submitted", "confirmed", "error"
    /// </summary>
    [MaxLength(20)]
    public string? ToyotaStatus { get; set; }

    /// <summary>
    /// Error message from Toyota API (if failed)
    /// </summary>
    [MaxLength(500)]
    public string? ToyotaErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when submitted to Toyota API
    /// </summary>
    public DateTime? ToyotaSubmittedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;

    public virtual ICollection<ShipmentLoadException> ShipmentLoadExceptions { get; set; } = new List<ShipmentLoadException>();
}
