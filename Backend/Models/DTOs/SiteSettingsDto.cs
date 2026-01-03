// Author: Hassan
// Date: 2025-01-03
// Description: DTOs for Site Settings API - Consolidated site-wide settings

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Site Settings DTO for API responses
/// Consolidated settings from Site, Dock Monitor, and Internal Kanban tabs
/// </summary>
public class SiteSettingsDto
{
    /// <summary>
    /// Setting ID (GUID)
    /// </summary>
    public Guid SettingId { get; set; }

    #region Site Settings Tab

    /// <summary>
    /// Plant location name (e.g., "Indiana Plant", "Michigan Plant")
    /// </summary>
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
    public bool EnablePreShipmentScan { get; set; }

    #endregion

    #region Dock Monitor Tab

    /// <summary>
    /// Behind threshold in minutes (default: 15)
    /// </summary>
    public int DockBehindThreshold { get; set; }

    /// <summary>
    /// Critical threshold in minutes (default: 30)
    /// </summary>
    public int DockCriticalThreshold { get; set; }

    /// <summary>
    /// Display mode: "FULL", "COMPACT", etc.
    /// </summary>
    public string DockDisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Refresh interval in milliseconds (default: 300000 = 5 minutes)
    /// </summary>
    public int DockRefreshInterval { get; set; }

    /// <summary>
    /// Order lookback hours - how far back to search for orders (default: 36)
    /// </summary>
    public int DockOrderLookbackHours { get; set; }

    #endregion

    #region Internal Kanban Tab

    /// <summary>
    /// Allow duplicate internal kanban scans
    /// </summary>
    public bool KanbanAllowDuplicates { get; set; }

    /// <summary>
    /// Duplicate window in hours - time period to check for duplicates (default: 24)
    /// </summary>
    public int KanbanDuplicateWindowHours { get; set; }

    /// <summary>
    /// Alert user when duplicate internal kanban is scanned
    /// </summary>
    public bool KanbanAlertOnDuplicate { get; set; }

    #endregion

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Request model for updating Site Settings
/// </summary>
public class UpdateSiteSettingsRequest
{
    #region Site Settings Tab

    /// <summary>
    /// Plant location name (e.g., "Indiana Plant", "Michigan Plant")
    /// </summary>
    [MaxLength(100, ErrorMessage = "PlantLocation cannot exceed 100 characters")]
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
    [Required(ErrorMessage = "EnablePreShipmentScan is required")]
    public bool EnablePreShipmentScan { get; set; }

    #endregion

    #region Dock Monitor Tab

    /// <summary>
    /// Behind threshold in minutes (default: 15)
    /// </summary>
    [Required(ErrorMessage = "DockBehindThreshold is required")]
    [Range(1, 240, ErrorMessage = "DockBehindThreshold must be between 1 and 240 minutes")]
    public int DockBehindThreshold { get; set; }

    /// <summary>
    /// Critical threshold in minutes (default: 30)
    /// </summary>
    [Required(ErrorMessage = "DockCriticalThreshold is required")]
    [Range(1, 480, ErrorMessage = "DockCriticalThreshold must be between 1 and 480 minutes")]
    public int DockCriticalThreshold { get; set; }

    /// <summary>
    /// Display mode: "FULL", "COMPACT", etc.
    /// </summary>
    [Required(ErrorMessage = "DockDisplayMode is required")]
    [MaxLength(50, ErrorMessage = "DockDisplayMode cannot exceed 50 characters")]
    public string DockDisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Refresh interval in milliseconds (default: 300000 = 5 minutes)
    /// </summary>
    [Required(ErrorMessage = "DockRefreshInterval is required")]
    [Range(60000, 600000, ErrorMessage = "DockRefreshInterval must be between 1 and 10 minutes")]
    public int DockRefreshInterval { get; set; }

    /// <summary>
    /// Order lookback hours - how far back to search for orders (default: 36)
    /// </summary>
    [Required(ErrorMessage = "DockOrderLookbackHours is required")]
    [Range(1, 168, ErrorMessage = "DockOrderLookbackHours must be between 1 and 168 hours")]
    public int DockOrderLookbackHours { get; set; }

    #endregion

    #region Internal Kanban Tab

    /// <summary>
    /// Allow duplicate internal kanban scans
    /// </summary>
    [Required(ErrorMessage = "KanbanAllowDuplicates is required")]
    public bool KanbanAllowDuplicates { get; set; }

    /// <summary>
    /// Duplicate window in hours - time period to check for duplicates (default: 24)
    /// </summary>
    [Required(ErrorMessage = "KanbanDuplicateWindowHours is required")]
    [Range(1, 168, ErrorMessage = "KanbanDuplicateWindowHours must be between 1 and 168 hours")]
    public int KanbanDuplicateWindowHours { get; set; }

    /// <summary>
    /// Alert user when duplicate internal kanban is scanned
    /// </summary>
    [Required(ErrorMessage = "KanbanAlertOnDuplicate is required")]
    public bool KanbanAlertOnDuplicate { get; set; }

    #endregion
}
