// Author: Hassan
// Date: 2025-12-06
// Description: Request DTO for recording skid build exceptions

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Request to record a skid build exception
/// </summary>
public class SkidBuildExceptionRequestDto
{
    /// <summary>
    /// Session ID (optional - can record exception without active session)
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Exception code: "10" (Revised Quantity), "11" (Modified QPC), "12" (Short Shipment), "20" (Non-Standard Packaging)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ExceptionCode { get; set; } = null!;

    /// <summary>
    /// Optional comments (max 100 chars)
    /// </summary>
    [MaxLength(100)]
    public string? Comments { get; set; }

    /// <summary>
    /// Skid number (optional - NULL for order-level exceptions)
    /// </summary>
    public int? SkidNumber { get; set; }

    /// <summary>
    /// User ID creating the exception (optional - can be retrieved from JWT token)
    /// </summary>
    public string? UserId { get; set; }
}
