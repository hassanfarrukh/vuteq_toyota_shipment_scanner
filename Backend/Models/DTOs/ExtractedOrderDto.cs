// Author: Hassan
// Date: 2025-12-04
// Description: DTO for extracted order header data from PDF and Excel

namespace Backend.Models.DTOs;

/// <summary>
/// DTO representing extracted order header information from PDF
/// </summary>
public class ExtractedOrderDto
{
    /// <summary>
    /// OWK Number (SupplierCode-DockCode-OrderSeries-OrderNumber)
    /// </summary>
    public string OwkNumber { get; set; } = null!;

    /// <summary>
    /// Supplier name from PDF
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Supplier code (e.g., 02806)
    /// </summary>
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Dock code (e.g., FL, H8, HL, ML, T6)
    /// </summary>
    public string? DockCode { get; set; }

    /// <summary>
    /// Transmit date from PDF
    /// </summary>
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// Arrive date/time
    /// </summary>
    public DateTime? ArriveDateTime { get; set; }

    /// <summary>
    /// Depart date/time
    /// </summary>
    public DateTime? DepartDateTime { get; set; }

    /// <summary>
    /// Unload date/time
    /// </summary>
    public DateTime? UnloadDateTime { get; set; }

    /// <summary>
    /// Number of line items in this order
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// List of extracted line items
    /// </summary>
    public List<ExtractedOrderItemDto> Items { get; set; } = new();

    // ========== Excel-specific fields ==========

    /// <summary>
    /// Real order number for QR matching (e.g., "2025120233")
    /// </summary>
    public string? RealOrderNumber { get; set; }

    /// <summary>
    /// Manifest number - unique skid/manifest identifier
    /// </summary>
    public long? ManifestNo { get; set; }

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    public string? PlantCode { get; set; }

    /// <summary>
    /// Planned route (e.g., "IDRE-06")
    /// </summary>
    public string? PlannedRoute { get; set; }

    /// <summary>
    /// Main route (e.g., "IEH6-33")
    /// </summary>
    public string? MainRoute { get; set; }

    /// <summary>
    /// Specialist code (nullable)
    /// </summary>
    public int? SpecialistCode { get; set; }

    /// <summary>
    /// MROS value (nullable)
    /// </summary>
    public int? Mros { get; set; }

    /// <summary>
    /// Planned pickup date - departure date (PLANNED PICKUP)
    /// Added 2025-12-09
    /// </summary>
    public DateTime? PlannedPickup { get; set; }
}
