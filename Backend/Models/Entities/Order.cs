// Author: Hassan
// Date: 2025-12-04
// Description: Entity representing tblOrders - Customer orders with Excel export compatibility fields

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models.Entities;

/// <summary>
/// Order entity representing customer orders
/// Simplified structure with RealOrderNumber as main identifier
/// </summary>
[Table("tblOrders")]
public class Order : AuditableEntity
{
    [Key]
    public Guid OrderId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Real order number for QR matching (e.g., "2025111701")
    /// Main order identifier
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RealOrderNumber { get; set; } = null!;

    /// <summary>
    /// Transmit date from PDF
    /// </summary>
    public DateTime? TransmitDate { get; set; }

    /// <summary>
    /// Supplier code (e.g., "02806")
    /// </summary>
    [MaxLength(20)]
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Dock code (e.g., "FL", "ML", "H8")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Foreign key to OrderUpload - links order to the upload that created it
    /// </summary>
    public Guid? UploadId { get; set; }

    /// <summary>
    /// Unload date
    /// </summary>
    public DateOnly? UnloadDate { get; set; }

    /// <summary>
    /// Unload time
    /// </summary>
    public TimeOnly? UnloadTime { get; set; }

    /// <summary>
    /// Planned pickup date/time from Excel - PLANNED PICKUP
    /// </summary>
    public DateTime? PlannedPickup { get; set; }

    /// <summary>
    /// SCS process stage from Excel - SCS PROCESS STAGE
    /// </summary>
    [MaxLength(50)]
    public string? ScsProcessStage { get; set; }

    /// <summary>
    /// Shipment sub status from Excel - SHIPMENT SUB STATUS
    /// </summary>
    [MaxLength(50)]
    public string? ShipmentSubStatus { get; set; }

    /// <summary>
    /// Sub status description from Excel - SUB STAT DESCRIPTION
    /// </summary>
    [MaxLength(200)]
    public string? SubStatDescription { get; set; }

    /// <summary>
    /// Shipment update by from Excel - UPDATE BY
    /// </summary>
    [MaxLength(100)]
    public string? ShipmentUpdateBy { get; set; }

    /// <summary>
    /// Shipment update date from Excel - UPDATE DATE
    /// </summary>
    public DateTime? ShipmentUpdateDate { get; set; }

    /// <summary>
    /// ASN status from Excel - ASN STATUS
    /// </summary>
    [MaxLength(50)]
    public string? AsnStatus { get; set; }

    /// <summary>
    /// ASN status description from Excel - ASN STATUS DESCRIPTION
    /// </summary>
    [MaxLength(200)]
    public string? AsnStatusDescription { get; set; }

    /// <summary>
    /// Status of the order in the workflow
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Planned;

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    [MaxLength(20)]
    public string? PlantCode { get; set; }

    /// <summary>
    /// Planned route (e.g., "IDRE-06")
    /// </summary>
    [MaxLength(50)]
    public string? PlannedRoute { get; set; }

    /// <summary>
    /// Main route (e.g., "IEH6-33")
    /// </summary>
    [MaxLength(50)]
    public string? MainRoute { get; set; }

    /// <summary>
    /// Specialist code (nullable)
    /// </summary>
    public int? SpecialistCode { get; set; }

    /// <summary>
    /// MROS value (nullable)
    /// </summary>
    public int? Mros { get; set; }

    /// <summary>
    /// Actual route (NULL for pending)
    /// </summary>
    [MaxLength(50)]
    public string? ActualRoute { get; set; }

    /// <summary>
    /// Actual pickup date (NULL for pending)
    /// </summary>
    public DateTime? ActualPickupDate { get; set; }

    /// <summary>
    /// Trailer identifier (NULL for pending)
    /// </summary>
    [MaxLength(50)]
    public string? Trailer { get; set; }

    // Shipment Load Fields - Added 2025-12-08
    /// <summary>
    /// Seal number for shipment security
    /// </summary>
    [MaxLength(50)]
    public string? SealNumber { get; set; }

    /// <summary>
    /// Driver name for shipment
    /// </summary>
    [MaxLength(200)]
    public string? DriverName { get; set; }

    /// <summary>
    /// Carrier/shipping company name
    /// </summary>
    [MaxLength(100)]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Shipment confirmation number
    /// </summary>
    [MaxLength(100)]
    public string? ShipmentConfirmation { get; set; }

    /// <summary>
    /// Timestamp when shipment was loaded
    /// </summary>
    public DateTime? ShipmentLoadedAt { get; set; }

    /// <summary>
    /// Additional notes for the shipment
    /// </summary>
    [MaxLength(500)]
    public string? ShipmentNotes { get; set; }

    // Navigation properties
    public virtual ICollection<PlannedItem> PlannedItems { get; set; } = new List<PlannedItem>();
    public virtual ICollection<SkidBuildSession> SkidBuildSessions { get; set; } = new List<SkidBuildSession>();
    public virtual ICollection<SkidBuildException> SkidBuildExceptions { get; set; } = new List<SkidBuildException>();
}
