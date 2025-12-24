// Author: Hassan
// Date: 2025-12-01
// Description: DTO for Order list with TotalParts count

namespace Backend.Models.DTOs;

/// <summary>
/// DTO for displaying order list with parts count
/// </summary>
public class OrderListDto
{
    public Guid OrderId { get; set; }
    public string RealOrderNumber { get; set; } = null!;
    public int TotalParts { get; set; }
    public string DockCode { get; set; } = null!;
    public DateTime? DepartureDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Status { get; set; }
    public Guid? UploadId { get; set; }
    public string? PlannedRoute { get; set; }
    public string? MainRoute { get; set; }
}
