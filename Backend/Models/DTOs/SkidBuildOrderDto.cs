// Author: Hassan
// Date: 2025-12-06
// Description: DTO for Skid Build order with planned items

namespace Backend.Models.DTOs;

/// <summary>
/// Order details for Skid Build workflow
/// </summary>
public class SkidBuildOrderDto
{
    /// <summary>
    /// Order ID (GUID)
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Order number (e.g., "2023080205")
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code (e.g., "V8", "FL")
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Supplier code (e.g., "02806")
    /// </summary>
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    public string? PlantCode { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// List of planned items for this order
    /// </summary>
    public List<SkidBuildPlannedItemDto> PlannedItems { get; set; } = new List<SkidBuildPlannedItemDto>();
}
