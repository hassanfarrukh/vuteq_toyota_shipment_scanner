// Author: Hassan
// Date: 2025-12-06
// Description: DTO for Skid Build session

namespace Backend.Models.DTOs;

/// <summary>
/// Skid Build session details
/// </summary>
public class SkidBuildSessionDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Current skid number
    /// </summary>
    public int? SkidNumber { get; set; }

    /// <summary>
    /// Session status (active, completed, cancelled)
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// User ID who started the session
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// When the session was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the session was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Confirmation number (if completed)
    /// </summary>
    public string? ConfirmationNumber { get; set; }
}
