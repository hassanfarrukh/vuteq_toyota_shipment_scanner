// Author: Hassan
// Date: 2025-11-24
// Description: DTO for Warehouse entity - used for API responses

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Warehouse Data Transfer Object for API responses
/// </summary>
public class WarehouseDto
{
    /// <summary>
    /// Unique identifier for the warehouse
    /// </summary>
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// Warehouse code (unique identifier)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Warehouse name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Warehouse address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State (US state code)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// ZIP code
    /// </summary>
    public string? Zip { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Contact person name
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Office code (Foreign Key to Office)
    /// </summary>
    public string? Office { get; set; }

    /// <summary>
    /// Indicates if the warehouse is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request model for creating a new warehouse
/// </summary>
public class CreateWarehouseRequest
{
    /// <summary>
    /// Warehouse code (unique identifier) - Required
    /// </summary>
    [Required(ErrorMessage = "Warehouse code is required")]
    [MaxLength(20, ErrorMessage = "Warehouse code cannot exceed 20 characters")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Warehouse name - Required
    /// </summary>
    [Required(ErrorMessage = "Warehouse name is required")]
    [MaxLength(200, ErrorMessage = "Warehouse name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Warehouse address - Required
    /// </summary>
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = null!;

    /// <summary>
    /// City - Required
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = null!;

    /// <summary>
    /// State (US state code) - Required
    /// </summary>
    [Required(ErrorMessage = "State is required")]
    [MaxLength(2, ErrorMessage = "State code must be 2 characters")]
    [MinLength(2, ErrorMessage = "State code must be 2 characters")]
    public string State { get; set; } = null!;

    /// <summary>
    /// ZIP code - Required
    /// </summary>
    [Required(ErrorMessage = "ZIP code is required")]
    [MaxLength(20, ErrorMessage = "ZIP code cannot exceed 20 characters")]
    public string Zip { get; set; } = null!;

    /// <summary>
    /// Office code (Foreign Key to Office) - Required
    /// </summary>
    [Required(ErrorMessage = "Office code is required")]
    [MaxLength(20, ErrorMessage = "Office code cannot exceed 20 characters")]
    public string Office { get; set; } = null!;

    /// <summary>
    /// Phone number - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact person name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Contact name cannot exceed 200 characters")]
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? ContactEmail { get; set; }
}

/// <summary>
/// Request model for updating an existing warehouse
/// </summary>
public class UpdateWarehouseRequest
{
    /// <summary>
    /// Warehouse address - Required
    /// </summary>
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = null!;

    /// <summary>
    /// City - Required
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = null!;

    /// <summary>
    /// State (US state code) - Required
    /// </summary>
    [Required(ErrorMessage = "State is required")]
    [MaxLength(2, ErrorMessage = "State code must be 2 characters")]
    [MinLength(2, ErrorMessage = "State code must be 2 characters")]
    public string State { get; set; } = null!;

    /// <summary>
    /// ZIP code - Required
    /// </summary>
    [Required(ErrorMessage = "ZIP code is required")]
    [MaxLength(20, ErrorMessage = "ZIP code cannot exceed 20 characters")]
    public string Zip { get; set; } = null!;

    /// <summary>
    /// Office code (Foreign Key to Office) - Required
    /// </summary>
    [Required(ErrorMessage = "Office code is required")]
    [MaxLength(20, ErrorMessage = "Office code cannot exceed 20 characters")]
    public string Office { get; set; } = null!;

    /// <summary>
    /// Phone number - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact person name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Contact name cannot exceed 200 characters")]
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? ContactEmail { get; set; }
}
