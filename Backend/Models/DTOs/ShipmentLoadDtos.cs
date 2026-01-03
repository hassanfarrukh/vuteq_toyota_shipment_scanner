// Author: Hassan
// Date: 2025-12-17
// Description: DTOs for Shipment Load operations - Toyota SCS Trailer API integration

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

// ===== SESSION MANAGEMENT DTOs =====

/// <summary>
/// Request DTO for starting/resuming a shipment load session
/// </summary>
public class StartSessionRequestDto
{
    [Required]
    [MaxLength(50)]
    public string RouteNumber { get; set; } = null!;

    [Required]
    [MaxLength(5)]
    public string SupplierCode { get; set; } = null!;

    [Required]
    public DateTime PickupDateTime { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(12)]
    public string? OrderNumber { get; set; }

    [MaxLength(3)]
    public string? DockCode { get; set; }
}

/// <summary>
/// Response DTO for session start/resume
/// </summary>
public class SessionResponseDto
{
    public Guid SessionId { get; set; }
    public string RouteNumber { get; set; } = null!;
    public string? Route { get; set; }
    public string? Run { get; set; }
    public string? SupplierCode { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public string Status { get; set; } = null!;
    public string? TrailerNumber { get; set; }
    public string? SealNumber { get; set; }
    public string? LpCode { get; set; }
    public string? DriverFirstName { get; set; }
    public string? DriverLastName { get; set; }
    public string? SupplierFirstName { get; set; }
    public string? SupplierLastName { get; set; }
    public List<ShipmentLoadOrderDto> Orders { get; set; } = new List<ShipmentLoadOrderDto>();
    public List<ExceptionDto> Exceptions { get; set; } = new List<ExceptionDto>();
    public List<PlannedSkidDto> PlannedSkids { get; set; } = new List<PlannedSkidDto>();
    public bool IsResumed { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ScannedOrderSkidCount { get; set; }
}

/// <summary>
/// Request DTO for updating session trailer information
/// </summary>
public class UpdateSessionRequestDto
{
    [Required]
    [MaxLength(50)]
    public string TrailerNumber { get; set; } = null!;

    [MaxLength(50)]
    public string? SealNumber { get; set; }

    [MaxLength(6)]
    public string? LpCode { get; set; }

    [Required]
    [MaxLength(9)]
    public string DriverFirstName { get; set; } = null!;

    [Required]
    [MaxLength(12)]
    public string DriverLastName { get; set; } = null!;

    [MaxLength(9)]
    public string? SupplierFirstName { get; set; }

    [MaxLength(12)]
    public string? SupplierLastName { get; set; }
}

/// <summary>
/// Request DTO for adding exception
/// </summary>
public class AddExceptionRequestDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExceptionType { get; set; } = null!;

    [MaxLength(500)]
    public string? Comments { get; set; }

    [MaxLength(50)]
    public string? RelatedSkidId { get; set; }

    [Required]
    public Guid CreatedByUserId { get; set; }
}

/// <summary>
/// Exception DTO
/// </summary>
public class ExceptionDto
{
    public Guid ExceptionId { get; set; }
    public string ExceptionType { get; set; } = null!;
    public string? Comments { get; set; }
    public string? RelatedSkidId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== TOYOTA API PAYLOAD DTOs =====

/// <summary>
/// Toyota API Trailer payload (root level)
/// </summary>
public class ToyotaTrailerPayload
{
    public string supplier { get; set; } = null!;
    public string route { get; set; } = null!;
    public string run { get; set; } = null!;
    public string trailerNumber { get; set; } = null!;
    public bool dropHook { get; set; } = false; // HARDCODED - VUTEQ business rule
    public string? sealNumber { get; set; }
    public string? lpCode { get; set; }
    public string? driverTeamFirstName { get; set; }
    public string? driverTeamLastName { get; set; }
    public string? supplierTeamFirstName { get; set; }
    public string? supplierTeamLastName { get; set; }
    public List<ToyotaExceptionDto> exceptions { get; set; } = new List<ToyotaExceptionDto>();
    public List<ToyotaOrderDto> orders { get; set; } = new List<ToyotaOrderDto>();
}

/// <summary>
/// Toyota API Order payload
/// </summary>
public class ToyotaOrderDto
{
    public string order { get; set; } = null!;
    public string supplier { get; set; } = null!;
    public string plant { get; set; } = null!;
    public string dock { get; set; } = null!;
    public string pickUp { get; set; } = null!; // RFC3339 format: yyyy-MM-ddThh:mm
    public List<ToyotaSkidDto> skids { get; set; } = new List<ToyotaSkidDto>();
}

/// <summary>
/// Toyota API Skid payload
/// </summary>
public class ToyotaSkidDto
{
    public string skidId { get; set; } = null!;
    public string? palletization { get; set; }
    public bool skidCut { get; set; } = false;
    public List<ToyotaExceptionDto> exceptions { get; set; } = new List<ToyotaExceptionDto>();
}

/// <summary>
/// Toyota API Exception payload
/// </summary>
public class ToyotaExceptionDto
{
    public string exceptionCode { get; set; } = null!;
    public string? comments { get; set; }
}

/// <summary>
/// Toyota API Response
/// </summary>
public class ToyotaApiResponse
{
    public string? confirmationNumber { get; set; }
    public string? status { get; set; }
    public string? message { get; set; }
    public List<ToyotaApiError>? errors { get; set; }
}

/// <summary>
/// Toyota API Error detail
/// </summary>
public class ToyotaApiError
{
    public string? field { get; set; }
    public string? message { get; set; }
    public string? keyObject { get; set; }
}

// ===== EXISTING DTOs (updated) =====

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
    /// Session ID for the active shipment load session
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

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
    /// Palletization code from manifest scan
    /// </summary>
    [MaxLength(2)]
    public string? PalletizationCode { get; set; }

    /// <summary>
    /// MROS value from manifest scan
    /// </summary>
    [MaxLength(2)]
    public string? Mros { get; set; }

    /// <summary>
    /// Skid ID from manifest scan
    /// </summary>
    [MaxLength(4)]
    public string? SkidId { get; set; }
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
    /// Session ID to complete
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// User completing the shipment
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
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

/// <summary>
/// Response DTO for order validation without session creation
/// </summary>
public class ValidateOrderResponseDto
{
    /// <summary>
    /// Validation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Order ID
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
    /// Plant code
    /// </summary>
    public string PlantCode { get; set; } = null!;

    /// <summary>
    /// Supplier code
    /// </summary>
    public string SupplierCode { get; set; } = null!;

    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Indicates if skid-build is complete (Status >= SkidBuilt)
    /// </summary>
    public bool SkidBuildComplete { get; set; }

    /// <summary>
    /// Number of skids from tblSkidScans
    /// </summary>
    public int SkidCount { get; set; }

    /// <summary>
    /// Toyota confirmation number if skid-build was confirmed
    /// </summary>
    public string? ToyotaConfirmationNumber { get; set; }

    /// <summary>
    /// Toyota SHIPMENT confirmation number (set when shipment load is completed)
    /// This is different from ToyotaConfirmationNumber which is for Skid Build
    /// </summary>
    public string? ToyotaShipmentConfirmationNumber { get; set; }
}
