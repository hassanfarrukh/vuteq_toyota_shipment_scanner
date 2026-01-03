// Author: Hassan
// Date: 2025-12-31
// Description: Service interface for Pre-Shipment operations - Manifest-based session creation before driver arrives

using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Services;

/// <summary>
/// Interface for Pre-Shipment service operations
/// Pre-Shipment allows warehouse staff to prepare shipments before driver arrives
/// </summary>
public interface IPreShipmentService
{
    /// <summary>
    /// Create Pre-Shipment session from manifest scan
    /// Parses manifest barcode, extracts order number, queries route, creates session
    /// </summary>
    Task<ApiResponse<CreateFromManifestResponseDto>> CreateFromManifestAsync(CreateFromManifestRequestDto request);

    /// <summary>
    /// Get list of all Pre-Shipment sessions
    /// </summary>
    Task<ApiResponse<List<PreShipmentListItemDto>>> GetListAsync();

    /// <summary>
    /// Get Pre-Shipment session details by ID
    /// </summary>
    Task<ApiResponse<SessionResponseDto>> GetSessionAsync(Guid sessionId);

    /// <summary>
    /// Update Pre-Shipment session with trailer/driver info
    /// </summary>
    Task<ApiResponse<SessionResponseDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequestDto request);

    /// <summary>
    /// Complete Pre-Shipment session and submit to Toyota API
    /// </summary>
    Task<ApiResponse<PreShipmentCompleteResponseDto>> CompleteAsync(PreShipmentCompleteRequestDto request);

    /// <summary>
    /// Delete incomplete Pre-Shipment session
    /// </summary>
    Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId);
}
