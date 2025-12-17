// Author: Hassan
// Date: 2025-12-17
// Description: DTO for order skids response

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for order skids
/// </summary>
public class OrderSkidsResponseDto
{
    /// <summary>
    /// Order number (RealOrderNumber)
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Order ID (GUID)
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// List of distinct skids built for this order
    /// </summary>
    public List<SkidDto> Skids { get; set; } = new();

    /// <summary>
    /// Total count of distinct skids
    /// </summary>
    public int TotalSkids { get; set; }
}

/// <summary>
/// DTO representing a single skid
/// </summary>
public class SkidDto
{
    /// <summary>
    /// Combined skid identifier (e.g., "001A", "002B")
    /// </summary>
    public string SkidId { get; set; } = null!;

    /// <summary>
    /// Skid number (first 3 digits, e.g., "001")
    /// </summary>
    public string SkidNumber { get; set; } = null!;

    /// <summary>
    /// Skid side (4th character, e.g., "A" or "B")
    /// </summary>
    public string? SkidSide { get; set; }

    /// <summary>
    /// Palletization code (e.g., "LB", "CA")
    /// </summary>
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// First scan timestamp for this skid
    /// </summary>
    public DateTime? ScannedAt { get; set; }
}
