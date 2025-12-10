// Author: Hassan
// Date: 2025-12-04
// Description: DTOs for Excel parsing results from TSCS Compliance Dashboard

namespace Backend.Models.DTOs;

/// <summary>
/// Result of parsing TSCS Compliance Dashboard Excel file
/// </summary>
public class ExcelParseResult
{
    /// <summary>
    /// Summary data from NAMC Detail sheet
    /// </summary>
    public NamcSummary Summary { get; set; } = new NamcSummary();

    /// <summary>
    /// List of pending shipments from Shipment Detail sheet
    /// </summary>
    public List<ParsedShipment> Shipments { get; set; } = new List<ParsedShipment>();
}

/// <summary>
/// Summary data from NAMC Detail sheet
/// Contains aggregate metrics for supplier's shipment performance
/// </summary>
public class NamcSummary
{
    /// <summary>
    /// Supplier code (e.g., 5471)
    /// </summary>
    public int SupplierCode { get; set; }

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    public string PlantCode { get; set; } = string.Empty;

    /// <summary>
    /// Total planned shipments
    /// </summary>
    public int TotalPlanned { get; set; }

    /// <summary>
    /// Total shipped
    /// </summary>
    public int TotalShipped { get; set; }

    /// <summary>
    /// Total shorted
    /// </summary>
    public int TotalShorted { get; set; }

    /// <summary>
    /// Total late shipments
    /// </summary>
    public int TotalLate { get; set; }

    /// <summary>
    /// Total pending shipments
    /// </summary>
    public int TotalPending { get; set; }
}

/// <summary>
/// Parsed shipment data from Shipment Detail sheet (Pending status only)
/// Contains both order-level and item-level information
/// </summary>
public class ParsedShipment
{
    // ========== Order-Level Fields ==========

    /// <summary>
    /// Manifest number - unique skid/manifest identifier
    /// </summary>
    public long ManifestNo { get; set; }

    /// <summary>
    /// Supplier code (e.g., "5471")
    /// </summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>
    /// Dock code (e.g., "H6", "H8", "ML")
    /// </summary>
    public string DockCode { get; set; } = string.Empty;

    /// <summary>
    /// Real order number (e.g., "2025120233")
    /// </summary>
    public string RealOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Order date (Transmit date)
    /// </summary>
    public DateTime? TransmitDate { get; set; }

    /// <summary>
    /// Unload date (date portion only)
    /// </summary>
    public DateOnly? UnloadDate { get; set; }

    /// <summary>
    /// Unload time (time portion only)
    /// </summary>
    public TimeOnly? UnloadTime { get; set; }

    /// <summary>
    /// Plant code (e.g., "02TMI")
    /// </summary>
    public string PlantCode { get; set; } = string.Empty;

    /// <summary>
    /// Planned route (e.g., "IDRE-06")
    /// </summary>
    public string PlannedRoute { get; set; } = string.Empty;

    /// <summary>
    /// Main route (e.g., "IEH6-33")
    /// </summary>
    public string MainRoute { get; set; } = string.Empty;

    /// <summary>
    /// Specialist code (nullable, e.g., 14)
    /// </summary>
    public int? SpecialistCode { get; set; }

    /// <summary>
    /// MROS value (nullable, e.g., 33)
    /// </summary>
    public int? Mros { get; set; }

    // ========== Item-Level Fields ==========

    /// <summary>
    /// Part number (e.g., "6241308080C0")
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Kanban number (e.g., "HD29")
    /// </summary>
    public string KanbanNumber { get; set; } = string.Empty;

    /// <summary>
    /// Quantity per container (QPC)
    /// </summary>
    public int Qpc { get; set; }

    /// <summary>
    /// Total boxes planned
    /// </summary>
    public int TotalBoxPlanned { get; set; }

    /// <summary>
    /// Palletization code (e.g., "CA", "U8", "IA")
    /// </summary>
    public string PalletizationCode { get; set; } = string.Empty;

    /// <summary>
    /// TSCS external order ID (e.g., 48249852)
    /// </summary>
    public long ExternalOrderId { get; set; }

    /// <summary>
    /// Planned pickup date - departure date (PLANNED PICKUP)
    /// </summary>
    public DateTime? PlannedPickup { get; set; }

    /// <summary>
    /// Shortage/overage quantity (SHORT/OVER)
    /// </summary>
    public int ShortOver { get; set; }

    /// <summary>
    /// Total pieces count (PIECES)
    /// </summary>
    public int Pieces { get; set; }
}
