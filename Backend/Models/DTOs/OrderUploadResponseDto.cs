// Author: Hassan
// Date: 2025-12-01
// Description: DTO for order upload response with extracted data summary

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for order file upload with extraction summary
/// </summary>
public class OrderUploadResponseDto
{
    /// <summary>
    /// Upload record ID
    /// </summary>
    public Guid UploadId { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Upload status: pending, processing, success, error
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Number of orders extracted from PDF
    /// </summary>
    public int OrdersCreated { get; set; }

    /// <summary>
    /// Total number of line items extracted
    /// </summary>
    public int TotalItemsCreated { get; set; }

    /// <summary>
    /// Number of orders skipped (duplicates)
    /// </summary>
    public int OrdersSkipped { get; set; }

    /// <summary>
    /// List of order numbers that were skipped (already exist)
    /// </summary>
    public List<string> SkippedOrderNumbers { get; set; } = new();

    /// <summary>
    /// List of extracted orders
    /// </summary>
    public List<ExtractedOrderDto> ExtractedOrders { get; set; } = new();

    /// <summary>
    /// Any error messages during processing
    /// </summary>
    public string? ErrorMessage { get; set; }
}
