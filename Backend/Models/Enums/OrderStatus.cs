// Author: Hassan
// Date: 2025-12-01
// Description: Enum for Order status workflow (renamed from PlannedItemStatus)

namespace Backend.Models.Enums;

/// <summary>
/// Status enum for Order lifecycle from planned to shipped
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// PDF uploaded, awaiting processing
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Skid build in progress
    /// </summary>
    SkidBuilding = 1,

    /// <summary>
    /// Skid build complete
    /// </summary>
    SkidBuilt = 2,

    /// <summary>
    /// Ready for shipment load
    /// </summary>
    ReadyToShip = 3,

    /// <summary>
    /// Being loaded onto truck
    /// </summary>
    ShipmentLoading = 4,

    /// <summary>
    /// Loaded and shipped
    /// </summary>
    Shipped = 5,

    /// <summary>
    /// Error from Toyota API during Skid Build submission
    /// </summary>
    SkidBuildError = 6,

    /// <summary>
    /// Error from Toyota API during Shipment Load submission
    /// </summary>
    ShipmentError = 7
}
