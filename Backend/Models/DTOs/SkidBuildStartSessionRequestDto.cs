// Author: Hassan
// Date: 2025-12-06
// Description: Request DTO for starting a skid build session

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Request to start a new skid build session
/// </summary>
public class SkidBuildStartSessionRequestDto
{
    /// <summary>
    /// Order ID
    /// </summary>
    [Required(ErrorMessage = "OrderId is required")]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Skid number (e.g., 1, 2, 3)
    /// </summary>
    [Required(ErrorMessage = "SkidNumber is required")]
    [Range(1, int.MaxValue, ErrorMessage = "SkidNumber must be greater than 0")]
    public int SkidNumber { get; set; }

    /// <summary>
    /// User ID starting the session (optional - can be retrieved from JWT token)
    /// </summary>
    public string? UserId { get; set; }
}
