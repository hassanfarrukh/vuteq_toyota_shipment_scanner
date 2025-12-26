// Author: Hassan
// Date: 2025-12-24
// Description: DTOs for Dock Monitor API - Real-time order data for dock monitor display

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Response for a single order in the dock monitor
/// </summary>
public class DockMonitorOrderDto
{
    /// <summary>
    /// Unique order identifier
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Order number (RealOrderNumber)
    /// </summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// Dock code (e.g., "FL", "ML", "H8")
    /// </summary>
    public string DockCode { get; set; } = null!;

    /// <summary>
    /// Destination derived from dock code or plant
    /// </summary>
    public string? Destination { get; set; }

    /// <summary>
    /// Supplier code
    /// </summary>
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Planned pickup date/time
    /// </summary>
    public DateTime? PlannedPickup { get; set; }

    /// <summary>
    /// Planned skid build time (calculated from PlannedPickup - typically 2 hours before)
    /// </summary>
    public DateTime? PlannedSkidBuild { get; set; }

    /// <summary>
    /// Completed skid build time (ToyotaSkidBuildSubmittedAt)
    /// </summary>
    public DateTime? CompletedSkidBuild { get; set; }

    /// <summary>
    /// Planned shipment load time (same as PlannedPickup or slightly before)
    /// </summary>
    public DateTime? PlannedShipmentLoad { get; set; }

    /// <summary>
    /// Completed shipment load time (ToyotaShipmentSubmittedAt)
    /// </summary>
    public DateTime? CompletedShipmentLoad { get; set; }

    /// <summary>
    /// Order status: COMPLETED, ON_TIME, BEHIND, CRITICAL, PROJECT_SHORT, SHORT_SHIPPED
    /// </summary>
    public string Status { get; set; } = "ON_TIME";

    /// <summary>
    /// Is this a supplement order
    /// </summary>
    public bool IsSupplementOrder { get; set; }

    /// <summary>
    /// Toyota skid build status: pending, submitted, confirmed, error
    /// </summary>
    public string? ToyotaSkidBuildStatus { get; set; }

    /// <summary>
    /// Toyota shipment status: pending, submitted, confirmed, error
    /// </summary>
    public string? ToyotaShipmentStatus { get; set; }
}

/// <summary>
/// Response for a single shipment (grouped orders by route)
/// </summary>
public class DockMonitorShipmentDto
{
    /// <summary>
    /// Route number (from ShipmentLoadSession or MainRoute)
    /// </summary>
    public string RouteNumber { get; set; } = null!;

    /// <summary>
    /// Run code (last 2 chars of route or Run field)
    /// </summary>
    public string? Run { get; set; }

    /// <summary>
    /// Supplier code for the shipment
    /// </summary>
    public string? SupplierCode { get; set; }

    /// <summary>
    /// Pickup date/time from shipment session
    /// </summary>
    public DateTime? PickupDateTime { get; set; }

    /// <summary>
    /// Shipment status: active, completed, pending
    /// </summary>
    public string ShipmentStatus { get; set; } = "pending";

    /// <summary>
    /// Completion timestamp (if completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// List of orders in this shipment
    /// </summary>
    public List<DockMonitorOrderDto> Orders { get; set; } = new();
}

/// <summary>
/// Full dock monitor response
/// </summary>
public class DockMonitorResponseDto
{
    /// <summary>
    /// List of shipments with their orders
    /// </summary>
    public List<DockMonitorShipmentDto> Shipments { get; set; } = new();

    /// <summary>
    /// Total number of orders across all shipments
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Dock monitor settings applied to this data
    /// </summary>
    public DockMonitorSettingsDto Settings { get; set; } = null!;

    /// <summary>
    /// Timestamp when this data was generated
    /// </summary>
    public DateTime RefreshedAt { get; set; }
}
