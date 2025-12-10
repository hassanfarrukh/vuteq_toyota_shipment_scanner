// Author: Hassan
// Date: 2025-12-04
// Description: DTO for extracted order line item data from PDF and Excel

namespace Backend.Models.DTOs;

/// <summary>
/// DTO representing extracted line item from PDF
/// </summary>
public class ExtractedOrderItemDto
{
    /// <summary>
    /// Part number (e.g., 68101-0E120-00)
    /// </summary>
    public string PartNumber { get; set; } = null!;

    /// <summary>
    /// Part description (e.g., GLASS SUB-ASSY FR)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Lot quantity per kanban
    /// </summary>
    public int? LotQty { get; set; }

    /// <summary>
    /// Kanban number (e.g., FA99, FAA0)
    /// </summary>
    public string? KanbanNumber { get; set; }

    /// <summary>
    /// Total quantity ordered for this order number
    /// </summary>
    public int PlannedQty { get; set; }

    /// <summary>
    /// Raw kanban value from PDF for reference
    /// </summary>
    public string? RawKanbanValue { get; set; }

    // ========== Excel-specific fields ==========

    /// <summary>
    /// Quantity per container (QPC) - Excel field
    /// </summary>
    public int? Qpc { get; set; }

    /// <summary>
    /// Total boxes planned - Excel field
    /// </summary>
    public int? TotalBoxPlanned { get; set; }

    /// <summary>
    /// Palletization code (e.g., "CA", "U8", "IA")
    /// </summary>
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// TSCS external order ID (e.g., 48249852)
    /// </summary>
    public long? ExternalOrderId { get; set; }

    /// <summary>
    /// Manifest number - identifies which skid this item belongs to (MANIFEST_NO)
    /// Each item has its own ManifestNo from the Excel row
    /// </summary>
    public long ManifestNo { get; set; }

    /// <summary>
    /// Shortage/overage quantity (SHORT/OVER)
    /// Added 2025-12-09
    /// </summary>
    public int? ShortOver { get; set; }

    /// <summary>
    /// Total pieces count (PIECES)
    /// Added 2025-12-09
    /// </summary>
    public int? Pieces { get; set; }
}
