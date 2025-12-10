// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblInternalKanbanSettings - Internal Kanban system settings

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Internal Kanban Setting entity for internal kanban system configuration
/// </summary>
[Table("tblInternalKanbanSettings")]
public class InternalKanbanSetting : AuditableEntity
{
    [Key]
    public Guid SettingId { get; set; } = Guid.NewGuid();

    public bool AllowDuplicates { get; set; } = false;

    public int DuplicateWindow { get; set; } = 24; // hours

    public bool AlertOnDuplicate { get; set; } = true;

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }
}
