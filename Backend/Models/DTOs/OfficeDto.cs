// Author: Hassan
// Date: 2025-11-24
// Description: DTO for Office entity - used for API responses

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Office Data Transfer Object for API responses
/// </summary>
public class OfficeDto
{
    /// <summary>
    /// Unique identifier for the office
    /// </summary>
    public Guid OfficeId { get; set; }

    /// <summary>
    /// Office code (unique identifier)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Office name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Office address
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
    public string? Contact { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Indicates if the office is active
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
/// Request model for creating a new office
/// </summary>
public class CreateOfficeRequest
{
    /// <summary>
    /// Office code (unique identifier) - Required
    /// </summary>
    [Required(ErrorMessage = "Office code is required")]
    [MaxLength(20, ErrorMessage = "Office code cannot exceed 20 characters")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Office name - Required
    /// </summary>
    [Required(ErrorMessage = "Office name is required")]
    [MaxLength(200, ErrorMessage = "Office name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Office address - Required
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
    /// Phone number - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact person name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Contact name cannot exceed 200 characters")]
    public string? Contact { get; set; }

    /// <summary>
    /// Email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }
}

/// <summary>
/// Request model for updating an existing office
/// </summary>
public class UpdateOfficeRequest
{
    /// <summary>
    /// Office name - Required
    /// </summary>
    [Required(ErrorMessage = "Office name is required")]
    [MaxLength(200, ErrorMessage = "Office name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Office address - Required
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
    /// Phone number - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact person name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Contact name cannot exceed 200 characters")]
    public string? Contact { get; set; }

    /// <summary>
    /// Email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }
}
