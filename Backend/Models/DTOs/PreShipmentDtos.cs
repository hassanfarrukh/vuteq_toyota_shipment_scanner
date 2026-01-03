// Author: Hassan
// Date: 2025-12-31
// Description: DTOs for Pre-Shipment operations - Manifest-based session creation with Toyota API integration

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

// ===== PRE-SHIPMENT SESSION CREATION =====

/// <summary>
/// Request DTO for creating Pre-Shipment session from manifest scan
/// </summary>
public class CreateFromManifestRequestDto
{
    /// <summary>
    /// 44-byte manifest barcode
    /// Format: Plant(2) + Supplier(5) + Dock(2) + Order(10) + LoadId(2) + Palletization(2) + MROS(4) + SkidId(4) + etc.
    /// </summary>
    [Required]
    [MinLength(44)]
    [MaxLength(44)]
    public string ManifestBarcode { get; set; } = null!;

    /// <summary>
    /// User ID performing the scan
    /// </summary>
    [Required]
    public Guid ScannedBy { get; set; }
}

/// <summary>
/// Response DTO for creating Pre-Shipment session from manifest
/// </summary>
public class CreateFromManifestResponseDto
{
    /// <summary>
    /// Created session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Route number determined from order lookup
    /// </summary>
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// Route code (all except last 2 chars)
    /// </summary>
    public string Route { get; set; } = null!;

    /// <summary>
    /// Run number (last 2 chars)
    /// </summary>
    public string Run { get; set; } = null!;

    /// <summary>
    /// Supplier code from manifest
    /// </summary>
    public string SupplierCode { get; set; } = null!;

    /// <summary>
    /// List of all orders on this route
    /// </summary>
    public List<ShipmentLoadOrderDto> Orders { get; set; } = new List<ShipmentLoadOrderDto>();

    /// <summary>
    /// List of all planned skids on this route
    /// </summary>
    public List<PlannedSkidDto> PlannedSkids { get; set; } = new List<PlannedSkidDto>();

    /// <summary>
    /// Total number of orders on route
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total number of planned skids on route
    /// </summary>
    public int TotalSkids { get; set; }

    /// <summary>
    /// Session creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Planned skid information
/// </summary>
public class PlannedSkidDto
{
    /// <summary>
    /// Order number this skid belongs to
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Skid ID (e.g., "001A")
    /// </summary>
    public string SkidId { get; set; } = null!;

    /// <summary>
    /// Skid number (first 3 chars of SkidId, e.g., "001")
    /// </summary>
    public string SkidNumber { get; set; } = null!;

    /// <summary>
    /// Skid side (4th char of SkidId, e.g., "A")
    /// </summary>
    public string? SkidSide { get; set; }

    /// <summary>
    /// Palletization code (e.g., "LB")
    /// </summary>
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// Number of parts on this skid
    /// </summary>
    public int PartCount { get; set; }

    /// <summary>
    /// Is this skid already scanned
    /// </summary>
    public bool IsScanned { get; set; }
}

// ===== PRE-SHIPMENT LIST =====

/// <summary>
/// Pre-Shipment session list item DTO
/// </summary>
public class PreShipmentListItemDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Route number
    /// </summary>
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// Supplier code
    /// </summary>
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Session status: "active", "completed", "cancelled", "error"
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Total number of skids on this route
    /// </summary>
    public int TotalSkidCount { get; set; }

    /// <summary>
    /// Number of skids scanned so far
    /// </summary>
    public int ScannedSkidCount { get; set; }

    /// <summary>
    /// Session creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Trailer number (if assigned)
    /// </summary>
    public string? TrailerNumber { get; set; }

    /// <summary>
    /// Created by user
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Toyota API submission status
    /// </summary>
    public string? ToyotaStatus { get; set; }

    /// <summary>
    /// Toyota confirmation number
    /// </summary>
    public string? ToyotaConfirmationNumber { get; set; }
}

// ===== PRE-SHIPMENT SCAN SKID =====

/// <summary>
/// Request DTO for scanning skid in Pre-Shipment
/// </summary>
public class PreShipmentScanSkidRequestDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// 44-byte manifest barcode for the skid
    /// </summary>
    [Required]
    [MinLength(44)]
    [MaxLength(44)]
    public string ManifestBarcode { get; set; } = null!;

    /// <summary>
    /// User ID performing the scan
    /// </summary>
    [Required]
    public Guid ScannedBy { get; set; }
}

/// <summary>
/// Response DTO for scanning skid in Pre-Shipment
/// </summary>
public class PreShipmentScanSkidResponseDto
{
    /// <summary>
    /// Order number
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Skid ID
    /// </summary>
    public string SkidId { get; set; } = null!;

    /// <summary>
    /// Validation message
    /// </summary>
    public string ValidationMessage { get; set; } = null!;

    /// <summary>
    /// Scanned at timestamp
    /// </summary>
    public DateTime ScannedAt { get; set; }

    /// <summary>
    /// Total skids scanned so far
    /// </summary>
    public int TotalScannedSkids { get; set; }

    /// <summary>
    /// Total planned skids
    /// </summary>
    public int TotalPlannedSkids { get; set; }
}

// ===== PRE-SHIPMENT COMPLETION =====

/// <summary>
/// Request DTO for completing Pre-Shipment session
/// </summary>
public class PreShipmentCompleteRequestDto
{
    /// <summary>
    /// Session ID to complete
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// User completing the session
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
}

/// <summary>
/// Response DTO for Pre-Shipment completion
/// </summary>
public class PreShipmentCompleteResponseDto
{
    /// <summary>
    /// Toyota confirmation number
    /// </summary>
    public string ConfirmationNumber { get; set; } = null!;

    /// <summary>
    /// Route number
    /// </summary>
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// Trailer number
    /// </summary>
    public string TrailerNumber { get; set; } = null!;

    /// <summary>
    /// Total orders shipped
    /// </summary>
    public int TotalOrdersShipped { get; set; }

    /// <summary>
    /// Total skids shipped
    /// </summary>
    public int TotalSkidsShipped { get; set; }

    /// <summary>
    /// Completion timestamp
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// List of shipped order numbers
    /// </summary>
    public List<string> ShippedOrderNumbers { get; set; } = new List<string>();
}
