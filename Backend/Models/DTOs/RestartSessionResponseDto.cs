// Author: Hassan
// Date: 2026-01-04
// Description: Response DTO for session restart operations

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for session restart operations
/// Used by both Skid Build and Shipment Load restart endpoints
/// </summary>
public class RestartSessionResponseDto
{
    /// <summary>
    /// Indicates if the restart was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the restart result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Always null - user must start a new session manually
    /// </summary>
    public Guid? NewSessionId { get; set; }
}
