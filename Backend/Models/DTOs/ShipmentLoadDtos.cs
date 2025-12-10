// Author: Hassan
// Date: 2025-12-08
// Description: DTOs for Shipment Load operations - handles shipment loading workflow

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for route lookup - returns list of orders ready to ship for a route
/// </summary>
public class ShipmentLoadRouteResponseDto
{
    /// <summary>
    /// Route number
    /// </summary>
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// List of orders ready to ship for this route
    /// </summary>
    public List<ShipmentLoadOrderDto> Orders { get; set; } = new List<ShipmentLoadOrderDto>();

    /// <summary>
    /// Total number of orders
    /// </summary>
    public int TotalOrders { get; set; }
}

/// <summary>
/// Order details for Shipment Load workflow
/// </summary>
public class ShipmentLoadOrderDto
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
    /// Planned route
    /// </summary>
    public string? PlannedRoute { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Number of skids built for this order
    /// </summary>
    public int TotalSkids { get; set; }

    /// <summary>
    /// Indicates if this order has been scanned in current session
    /// </summary>
    public bool IsScanned { get; set; }
}

/// <summary>
/// Request DTO for scanning an order during shipment load
/// </summary>
public class ShipmentLoadScanRequestDto
{
    /// <summary>
    /// Order number to scan
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code for the order
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Route number being loaded
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// User performing the scan (optional, uses system user if not provided)
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// Response DTO for scan validation result
/// </summary>
public class ShipmentLoadScanResponseDto
{
    /// <summary>
    /// Order ID that was scanned
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Order number
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Status after scan
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Validation result message
    /// </summary>
    public string ValidationMessage { get; set; } = null!;

    /// <summary>
    /// Timestamp when scanned
    /// </summary>
    public DateTime ScannedAt { get; set; }
}

/// <summary>
/// Request DTO for completing shipment load
/// </summary>
public class ShipmentLoadCompleteRequestDto
{
    /// <summary>
    /// Route number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// Trailer number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TrailerNumber { get; set; } = null!;

    /// <summary>
    /// Seal number
    /// </summary>
    [MaxLength(50)]
    public string? SealNumber { get; set; }

    /// <summary>
    /// Driver name
    /// </summary>
    [MaxLength(200)]
    public string? DriverName { get; set; }

    /// <summary>
    /// Carrier name
    /// </summary>
    [MaxLength(100)]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(500)]
    public string? ShipmentNotes { get; set; }

    /// <summary>
    /// User completing the shipment (optional, uses system user if not provided)
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// Response DTO for shipment completion
/// </summary>
public class ShipmentLoadCompleteResponseDto
{
    /// <summary>
    /// Shipment confirmation number
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
    /// Number of orders shipped
    /// </summary>
    public int TotalOrdersShipped { get; set; }

    /// <summary>
    /// Completion timestamp
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// List of order numbers that were shipped
    /// </summary>
    public List<string> ShippedOrderNumbers { get; set; } = new List<string>();
}
