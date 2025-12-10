// Author: Hassan
// Date: 2025-12-06
// Description: DTO for planned items in Skid Build workflow

namespace Backend.Models.DTOs;

/// <summary>
/// Planned item details for Skid Build
/// </summary>
public class SkidBuildPlannedItemDto
{
    /// <summary>
    /// Planned item ID (GUID)
    /// </summary>
    public Guid PlannedItemId { get; set; }

    /// <summary>
    /// Part number (e.g., "681010E250")
    /// </summary>
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Kanban number (e.g., "VH98")
    /// </summary>
    public string? KanbanNumber { get; set; }

    /// <summary>
    /// Quantity per container (QPC)
    /// </summary>
    public int? Qpc { get; set; }

    /// <summary>
    /// Total boxes planned
    /// </summary>
    public int? TotalBoxPlanned { get; set; }

    /// <summary>
    /// Manifest number
    /// </summary>
    public long ManifestNo { get; set; }

    /// <summary>
    /// Palletization code (e.g., "LB")
    /// </summary>
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// Count of boxes already scanned for this item
    /// </summary>
    public int ScannedCount { get; set; }
}
