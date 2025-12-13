// Author: Hassan
// Date: 2025-12-12
// Updated: 2025-12-14 - Added Toyota Skid Build API fields (ConfirmationNumber, Status, ErrorMessage, SubmittedAt)
// Description: DTO for Skid Build order with items grouped by skid

namespace Backend.Models.DTOs;

/// <summary>
/// Order details for Skid Build workflow with items grouped by skid
/// </summary>
public class SkidBuildOrderGroupedDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string DockCode { get; set; } = null!;
    public string? SupplierCode { get; set; }
    public string? PlantCode { get; set; }
    public string Status { get; set; } = null!;
    public List<SkidGroupDto> Skids { get; set; } = new List<SkidGroupDto>();

    // Toyota Skid Build API Fields
    public string? ToyotaSkidBuildConfirmationNumber { get; set; }
    public string? ToyotaSkidBuildStatus { get; set; }
    public string? ToyotaSkidBuildErrorMessage { get; set; }
    public DateTime? ToyotaSkidBuildSubmittedAt { get; set; }
}

/// <summary>
/// Represents a skid (manifest) with its planned kanbans
/// </summary>
public class SkidGroupDto
{
    public string SkidId { get; set; } = null!;
    public long ManifestNo { get; set; }
    public string? PalletizationCode { get; set; }
    public List<SkidBuildPlannedItemDto> PlannedKanbans { get; set; } = new List<SkidBuildPlannedItemDto>();
}
