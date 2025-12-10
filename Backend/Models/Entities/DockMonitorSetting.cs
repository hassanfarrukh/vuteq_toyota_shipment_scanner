// Author: Hassan
// Date: 2025-12-01
// Description: Entity representing tblDockMonitorSettings - Global dock monitor settings (system-wide, not per-user)

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Dock Monitor Setting entity for global dock monitor settings.
/// This is a system-wide setting (one record for the entire deployment).
/// SelectedLocations represents which plant this deployment belongs to (e.g., "INDIANA", "MICHIGAN").
/// </summary>
[Table("tblDockMonitorSettings")]
public class DockMonitorSetting : AuditableEntity
{
    [Key]
    public Guid SettingId { get; set; } = Guid.NewGuid();

    // UserId is now nullable - for global settings, we use a sentinel value or leave it null
    // This maintains backwards compatibility with existing database schema
    public Guid? UserId { get; set; }

    public int BehindThreshold { get; set; } = 15; // minutes

    public int CriticalThreshold { get; set; } = 30; // minutes

    [MaxLength(50)]
    public string DisplayMode { get; set; } = "FULL";

    /// <summary>
    /// Plant location(s) for this deployment (e.g., "INDIANA", "MICHIGAN")
    /// Stored as JSON array
    /// </summary>
    public string? SelectedLocations { get; set; } // JSON array

    public int RefreshInterval { get; set; } = 300000; // milliseconds

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }
}
