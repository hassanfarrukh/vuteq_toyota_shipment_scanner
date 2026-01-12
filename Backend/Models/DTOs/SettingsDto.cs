// Author: Hassan
// Date: 2025-12-01
// Description: DTOs for Settings APIs - Internal Kanban and Dock Monitor settings

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Internal Kanban Settings DTO for API responses and requests
/// </summary>
public class InternalKanbanSettingsDto
{
    /// <summary>
    /// Setting ID (GUID)
    /// </summary>
    public Guid SettingId { get; set; }

    /// <summary>
    /// Allow duplicate kanbans within the duplicate window
    /// </summary>
    public bool AllowDuplicates { get; set; }

    /// <summary>
    /// Duplicate window in hours (time window to check for duplicates)
    /// </summary>
    public int DuplicateWindowHours { get; set; }

    /// <summary>
    /// Show alert when duplicate detected
    /// </summary>
    public bool AlertOnDuplicate { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Request model for updating Internal Kanban settings
/// </summary>
public class UpdateInternalKanbanSettingsRequest
{
    /// <summary>
    /// Allow duplicate kanbans within the duplicate window
    /// </summary>
    [Required(ErrorMessage = "AllowDuplicates is required")]
    public bool AllowDuplicates { get; set; }

    /// <summary>
    /// Duplicate window in hours (time window to check for duplicates)
    /// </summary>
    [Required(ErrorMessage = "DuplicateWindowHours is required")]
    [Range(1, 8760, ErrorMessage = "DuplicateWindowHours must be between 1 and 8760 hours (1 year)")]
    public int DuplicateWindowHours { get; set; }

    /// <summary>
    /// Show alert when duplicate detected
    /// </summary>
    [Required(ErrorMessage = "AlertOnDuplicate is required")]
    public bool AlertOnDuplicate { get; set; }
}

/// <summary>
/// Dock Monitor Settings DTO for API responses
/// Global settings (system-wide, not per-user)
/// </summary>
public class DockMonitorSettingsDto
{
    /// <summary>
    /// Setting ID (GUID)
    /// </summary>
    public Guid SettingId { get; set; }

    /// <summary>
    /// User ID (nullable - global settings have no specific user)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Behind threshold in minutes (yellow warning)
    /// </summary>
    public int BehindThreshold { get; set; }

    /// <summary>
    /// Critical threshold in minutes (red alert)
    /// </summary>
    public int CriticalThreshold { get; set; }

    /// <summary>
    /// Display mode: FULL, SHIPMENT_ONLY, SKID_ONLY, COMPLETION_ONLY
    /// </summary>
    public string DisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Selected locations (e.g., ["INDIANA", "MICHIGAN"])
    /// </summary>
    public List<string> SelectedLocations { get; set; } = new List<string>();

    /// <summary>
    /// Refresh interval in milliseconds
    /// </summary>
    public int RefreshInterval { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Request model for updating Dock Monitor settings
/// </summary>
public class UpdateDockMonitorSettingsRequest
{
    /// <summary>
    /// Behind threshold in minutes (yellow warning)
    /// </summary>
    [Required(ErrorMessage = "BehindThreshold is required")]
    [Range(1, 240, ErrorMessage = "BehindThreshold must be between 1 and 240 minutes")]
    public int BehindThreshold { get; set; }

    /// <summary>
    /// Critical threshold in minutes (red alert)
    /// </summary>
    [Required(ErrorMessage = "CriticalThreshold is required")]
    [Range(1, 480, ErrorMessage = "CriticalThreshold must be between 1 and 480 minutes")]
    public int CriticalThreshold { get; set; }

    /// <summary>
    /// Display mode: FULL, SHIPMENT_ONLY, SKID_ONLY, COMPLETION_ONLY
    /// </summary>
    [Required(ErrorMessage = "DisplayMode is required")]
    [RegularExpression("^(FULL|SHIPMENT_ONLY|SKID_ONLY|COMPLETION_ONLY)$",
        ErrorMessage = "DisplayMode must be FULL, SHIPMENT_ONLY, SKID_ONLY, or COMPLETION_ONLY")]
    public string DisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Selected locations (e.g., ["INDIANA", "MICHIGAN"])
    /// </summary>
    public List<string> SelectedLocations { get; set; } = new List<string>();
}
