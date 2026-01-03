// Author: Hassan
// Date: 2025-12-04
// Description: DTOs for PlannedItem responses

namespace Backend.Models.DTOs;

/// <summary>
/// DTO for planned item with complete order information
/// </summary>
public class PlannedItemWithOrderDto
{
    public Guid PlannedItemId { get; set; }
    public Guid OrderId { get; set; }
    public string RealOrderNumber { get; set; } = null!;
    public string DockCode { get; set; } = null!;
    public string PartNumber { get; set; } = null!;
    public int? Qpc { get; set; }
    public string? KanbanNumber { get; set; }
    public int? TotalBoxPlanned { get; set; }
    public long ManifestNo { get; set; }
    public string? PalletizationCode { get; set; }
    public long ExternalOrderId { get; set; }
    public int? ShortOver { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalScanned { get; set; }
    public int RemainingBoxes { get; set; }

    /// <summary>
    /// Internal kanban(s) scanned for this item (comma-separated if multiple)
    /// </summary>
    public string? InternalKanban { get; set; }
}
