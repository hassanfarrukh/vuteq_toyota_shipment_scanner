// Author: Hassan
// Date: 2025-12-06
// Updated: 2025-12-14 - Added Toyota API response fields
// Description: Response DTO for completed skid build

namespace Backend.Models.DTOs;

/// <summary>
/// Response when completing a skid build session
/// Includes Toyota API submission status
/// </summary>
public class SkidBuildCompleteResponseDto
{
    /// <summary>
    /// Confirmation number (Toyota confirmation number if successful, internal reference otherwise)
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

    // ===== TOYOTA API FIELDS =====

    /// <summary>
    /// Toyota API submission status: pending, confirmed, error
    /// </summary>
    public string? ToyotaSubmissionStatus { get; set; }

    /// <summary>
    /// Toyota API confirmation number (same as ConfirmationNumber if successful)
    /// </summary>
    public string? ToyotaConfirmationNumber { get; set; }

    /// <summary>
    /// Error message from Toyota API if submission failed
    /// </summary>
    public string? ToyotaErrorMessage { get; set; }
}
