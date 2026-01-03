// Author: Hassan
// Date: 2025-01-03
// Description: Entity representing tblSiteSettings - Consolidated site-wide settings (single row per deployment)

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Site Settings entity for consolidated site-wide configuration.
/// This is a single-row table for on-prem deployments (one record per site).
/// Combines Site, Dock Monitor, and Internal Kanban settings.
/// </summary>
[Table("tblSiteSettings")]
public class SiteSettings
{
    [Key]
    public Guid SettingId { get; set; } = Guid.NewGuid();

    #region Site Settings Tab

    /// <summary>
    /// Plant location name (e.g., "Indiana Plant", "Michigan Plant")
    /// </summary>
    [MaxLength(100)]
    public string? PlantLocation { get; set; }

    /// <summary>
    /// Plant opening time (e.g., 06:00)
    /// </summary>
    public TimeOnly? PlantOpeningTime { get; set; }

    /// <summary>
    /// Plant closing time (e.g., 22:00)
    /// </summary>
    public TimeOnly? PlantClosingTime { get; set; }

    /// <summary>
    /// Enable PreShipment Scan tile on main screen
    /// </summary>
    public bool EnablePreShipmentScan { get; set; } = true;

    #endregion

    #region Dock Monitor Tab

    /// <summary>
    /// Behind threshold in minutes (default: 15)
    /// </summary>
    public int DockBehindThreshold { get; set; } = 15;

    /// <summary>
    /// Critical threshold in minutes (default: 30)
    /// </summary>
    public int DockCriticalThreshold { get; set; } = 30;

    /// <summary>
    /// Display mode: "FULL", "COMPACT", etc.
    /// </summary>
    [MaxLength(50)]
    public string DockDisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Refresh interval in milliseconds (default: 300000 = 5 minutes)
    /// </summary>
    public int DockRefreshInterval { get; set; } = 300000;

    /// <summary>
    /// Order lookback hours - how far back to search for orders (default: 36 hours)
    /// </summary>
    public int DockOrderLookbackHours { get; set; } = 36;

    #endregion

    #region Internal Kanban Tab

    /// <summary>
    /// Allow duplicate internal kanban scans
    /// </summary>
    public bool KanbanAllowDuplicates { get; set; } = false;

    /// <summary>
    /// Duplicate window in hours - time period to check for duplicates (default: 24)
    /// </summary>
    public int KanbanDuplicateWindowHours { get; set; } = 24;

    /// <summary>
    /// Alert user when duplicate internal kanban is scanned
    /// </summary>
    public bool KanbanAlertOnDuplicate { get; set; } = true;

    #endregion

    #region Audit Fields

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the record
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated the record
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    #endregion

    #region Navigation Properties

    [ForeignKey(nameof(CreatedBy))]
    public virtual UserMaster? CreatedByUser { get; set; }

    [ForeignKey(nameof(UpdatedBy))]
    public virtual UserMaster? UpdatedByUser { get; set; }

    #endregion
}
