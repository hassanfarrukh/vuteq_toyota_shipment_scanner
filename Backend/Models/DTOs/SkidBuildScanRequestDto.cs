// Author: Hassan
// Date: 2025-12-06
// Description: Request DTO for recording skid scans

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Request to record a skid scan
/// </summary>
public class SkidBuildScanRequestDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// Planned item ID being scanned
    /// </summary>
    [Required]
    public Guid PlannedItemId { get; set; }

    /// <summary>
    /// Skid number - first 3 digits from manifest QR (e.g., "001", "002", "003")
    /// CHANGED: From int to string with validation
    /// </summary>
    [Required]
    [MaxLength(3)]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "SkidNumber must be 3 numeric digits")]
    public string SkidNumber { get; set; } = null!;

    /// <summary>
    /// Skid side - 4th character from manifest QR (e.g., "A" or "B")
    /// NEW: Added for Toyota API compliance
    /// </summary>
    [MaxLength(1)]
    [RegularExpression(@"^[AB]$", ErrorMessage = "SkidSide must be A or B")]
    public string? SkidSide { get; set; }

    /// <summary>
    /// Raw SkidId from manifest QR for reference (e.g., "001B")
    /// NEW: Store original value from QR code
    /// </summary>
    [MaxLength(4)]
    public string? RawSkidId { get; set; }

    /// <summary>
    /// Box number from Toyota Kanban (separate from SkidNumber)
    /// </summary>
    [Required]
    [Range(1, 999, ErrorMessage = "BoxNumber must be between 1 and 999")]
    public int BoxNumber { get; set; }

    /// <summary>
    /// Line side address from Toyota Kanban (e.g., "SA-FDG")
    /// </summary>
    [MaxLength(20)]
    public string? LineSideAddress { get; set; }

    /// <summary>
    /// Palletization code for validation matching
    /// NEW: Added for Toyota API palletization code matching requirement
    /// </summary>
    [MaxLength(2)]
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// Internal kanban scanned (e.g., "MPE")
    /// </summary>
    [MaxLength(100)]
    public string? InternalKanban { get; set; }

    /// <summary>
    /// User ID performing the scan (optional - can be retrieved from JWT token)
    /// </summary>
    public string? UserId { get; set; }
}
