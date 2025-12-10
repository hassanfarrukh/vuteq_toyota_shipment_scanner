// Author: Hassan
// Date: 2025-11-23
// Description: Entity representing tblUserSessions - JWT token management and session tracking

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// User Session entity for JWT token management
/// </summary>
[Table("tblUserSessions")]
public class UserSession : AuditableEntity
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual UserMaster User { get; set; } = null!;
}
