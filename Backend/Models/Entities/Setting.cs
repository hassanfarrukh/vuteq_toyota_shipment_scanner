// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblSettings - Key-Value store for application settings

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Setting entity for application configuration key-value pairs
/// </summary>
[Table("tblSettings")]
public class Setting : AuditableEntity
{
    [Key]
    public Guid SettingId { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    [MaxLength(50)]
    public string? SettingType { get; set; }

    [NotMapped]
    public DateTime ModifiedAt
    {
        get => UpdatedAt ?? CreatedAt;
        set => UpdatedAt = value;
    }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserMaster? User { get; set; }
}
