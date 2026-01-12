// Author: Hassan
// Date: 2025-12-06
// Description: Entity representing tblSkidScans - Tracks each scan during skid build

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Skid Scan entity for tracking individual scans during skid build
/// One PlannedItem can have multiple SkidScan records (for continuation skids)
/// </summary>
[Table("tblSkidScans")]
public class SkidScan : AuditableEntity
{
    [Key]
    public Guid ScanId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PlannedItemId { get; set; }

    /// <summary>
    /// Skid number - first 3 digits from manifest QR "001B" → "001"
    /// CHANGED: From int to string for Toyota API compliance
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string SkidNumber { get; set; } = null!;

    /// <summary>
    /// Skid side - 4th character from manifest QR "001B" → "B"
    /// NEW: Added for Toyota API compliance (Side A or B)
    /// </summary>
    [MaxLength(1)]
    public string? SkidSide { get; set; }

    /// <summary>
    /// Raw SkidId from manifest QR for reference (e.g., "001B")
    /// NEW: Store original value from QR code
    /// </summary>
    [MaxLength(4)]
    public string? RawSkidId { get; set; }

    /// <summary>
    /// Box number from Toyota Kanban QR (position 152-155)
    /// </summary>
    public int BoxNumber { get; set; }

    /// <summary>
    /// Line side address from Toyota Kanban QR (position 55-64)
    /// </summary>
    [MaxLength(20)]
    public string? LineSideAddress { get; set; }

    /// <summary>
    /// Internal kanban scanned (e.g., "627300820100 HM550004771")
    /// </summary>
    [MaxLength(100)]
    public string? InternalKanban { get; set; }

    /// <summary>
    /// Parsed Serial Number from Internal Kanban (e.g., "0004771")
    /// Issue #4: Used for time-window duplicate checking (KanbanDuplicateWindowHours)
    /// </summary>
    [MaxLength(20)]
    public string? InternalKanbanSerial { get; set; }

    /// <summary>
    /// Palletization code for validation matching
    /// NEW: Added for Toyota API palletization code matching requirement
    /// </summary>
    [MaxLength(2)]
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// Indicates if this skid was cut (unpicked) during Shipment Load
    /// Used for skidCut field in Toyota API payload
    /// Default: false
    /// </summary>
    public bool IsSkidCut { get; set; } = false;

    /// <summary>
    /// Shipment Load Session ID - tracks which session this skid was scanned in during loading
    /// NULL = skid built but not scanned for shipment yet
    /// NOT NULL = skid scanned during shipment load in this session
    /// </summary>
    public Guid? ShipmentLoadSessionId { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.Now;

    public Guid? ScannedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PlannedItemId))]
    public virtual PlannedItem PlannedItem { get; set; } = null!;

    [ForeignKey(nameof(ScannedBy))]
    public virtual UserMaster? ScannedByUser { get; set; }
}
