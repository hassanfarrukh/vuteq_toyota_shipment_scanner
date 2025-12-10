// Author: Hassan
// Date: 2025-12-06
// Description: Response DTO for skid scan operations - prevents circular reference errors

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for recording a scan during skid build
/// </summary>
public class SkidBuildScanResponseDto
{
    /// <summary>
    /// Unique scan identifier
    /// </summary>
    public Guid ScanId { get; set; }

    /// <summary>
    /// Planned item that was scanned
    /// </summary>
    public Guid PlannedItemId { get; set; }

    /// <summary>
    /// Skid number - 3 digit string (e.g., "001", "002", "003")
    /// CHANGED: From int to string for Toyota API compliance
    /// </summary>
    public string SkidNumber { get; set; } = null!;

    /// <summary>
    /// Box number on the skid
    /// </summary>
    public int BoxNumber { get; set; }

    /// <summary>
    /// Line side address (SA-XXX format)
    /// </summary>
    public string? LineSideAddress { get; set; }

    /// <summary>
    /// Internal kanban identifier
    /// </summary>
    public string? InternalKanban { get; set; }

    /// <summary>
    /// Timestamp when scanned
    /// </summary>
    public DateTime ScannedAt { get; set; }

    /// <summary>
    /// User who performed the scan
    /// </summary>
    public Guid? ScannedBy { get; set; }
}
