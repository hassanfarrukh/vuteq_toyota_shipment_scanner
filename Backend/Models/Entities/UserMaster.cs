// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblUserMaster - User authentication and profile information

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// User Master entity for authentication and user management
/// </summary>
[Table("tblUserMaster")]
public class UserMaster : AuditableEntity
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = null!;

    [MaxLength(20)]
    public string? MenuLevel { get; set; }

    [MaxLength(50)]
    public string? Operation { get; set; }

    [MaxLength(50)]
    public string? LocationId { get; set; }

    [MaxLength(20)]
    public string? Code { get; set; }

    public bool IsSupervisor { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }

    // Navigation properties
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual ICollection<OrderUpload> OrderUploads { get; set; } = new List<OrderUpload>();
    public virtual ICollection<SkidBuildSession> SkidBuildSessions { get; set; } = new List<SkidBuildSession>();
    public virtual ICollection<ShipmentLoadSession> ShipmentLoadSessions { get; set; } = new List<ShipmentLoadSession>();
    public virtual ICollection<PreShipmentShipment> PreShipmentShipments { get; set; } = new List<PreShipmentShipment>();
    public virtual ICollection<SkidBuildDraft> SkidBuildDrafts { get; set; } = new List<SkidBuildDraft>();
    public virtual ICollection<ShipmentLoadDraft> ShipmentLoadDrafts { get; set; } = new List<ShipmentLoadDraft>();
    public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();
    // DockMonitorSetting is now global (system-wide), no longer per-user navigation property
}
