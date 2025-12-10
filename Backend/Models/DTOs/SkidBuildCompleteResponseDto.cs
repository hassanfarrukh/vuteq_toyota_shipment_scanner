// Author: Hassan
// Date: 2025-12-06
// Description: Response DTO for completed skid build

namespace Backend.Models.DTOs;

/// <summary>
/// Response when completing a skid build session
/// </summary>
public class SkidBuildCompleteResponseDto
{
    /// <summary>
    /// Confirmation number (e.g., "SKB-1701234567890-1234")
    /// </summary>
    public string ConfirmationNumber { get; set; } = null!;

    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Total number of items scanned
    /// </summary>
    public int TotalScanned { get; set; }

    /// <summary>
    /// Total number of exceptions recorded
    /// </summary>
    public int TotalExceptions { get; set; }

    /// <summary>
    /// When the session was completed
    /// </summary>
    public DateTime CompletedAt { get; set; }
}
