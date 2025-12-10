// Author: Hassan
// Date: 2025-11-23
// Description: Base entity class with audit tracking properties for automatic timestamp and user tracking

namespace Backend.Models.Entities;

/// <summary>
/// Base class for all entities that require audit tracking.
/// Provides common properties for creation and modification tracking.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// User ID of the person who created this record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID of the person who last modified this record
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when this record was last modified
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
