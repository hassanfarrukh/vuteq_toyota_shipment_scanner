// Author: Hassan
// Date: 2025-11-24
// Description: DTO for User entity - used for API responses

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// User Data Transfer Object for API responses
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID (unique identifier)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User full name (display name)
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// User nickname
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Notification recipient name
    /// </summary>
    public string? NotificationName { get; set; }

    /// <summary>
    /// Notification email address
    /// </summary>
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Indicates if user is a supervisor
    /// </summary>
    public bool Supervisor { get; set; }

    /// <summary>
    /// Menu access level (Admin, Scanner, Operation)
    /// </summary>
    public string? MenuLevel { get; set; }

    /// <summary>
    /// User operation type (Warehouse, Office, Administration)
    /// </summary>
    public string? Operation { get; set; }

    /// <summary>
    /// Code - Can be Office Code or Warehouse Code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = null!;

    /// <summary>
    /// Indicates if the user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

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
/// Request model for creating a new user
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Username for login - Required
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Password - Required
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// User full name - Optional (defaults to Username if not provided)
    /// </summary>
    [MaxLength(200, ErrorMessage = "User name cannot exceed 200 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// User nickname - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Nickname cannot exceed 200 characters")]
    public string? NickName { get; set; }

    /// <summary>
    /// User email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }

    /// <summary>
    /// Notification recipient name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Notification name cannot exceed 200 characters")]
    public string? NotificationName { get; set; }

    /// <summary>
    /// Notification email address - Optional
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid notification email format")]
    [MaxLength(200, ErrorMessage = "Notification email cannot exceed 200 characters")]
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Indicates if user is a supervisor - Optional (default: false)
    /// </summary>
    public bool? Supervisor { get; set; }

    /// <summary>
    /// Menu access level - Optional (default: Scanner)
    /// </summary>
    [MaxLength(20, ErrorMessage = "Menu level cannot exceed 20 characters")]
    public string? MenuLevel { get; set; }

    /// <summary>
    /// User operation type - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Operation cannot exceed 50 characters")]
    public string? Operation { get; set; }

    /// <summary>
    /// Code - Can be Office Code or Warehouse Code - Optional
    /// </summary>
    [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
    public string? Code { get; set; }
}

/// <summary>
/// Request model for updating an existing user
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Username for login - Optional (only if changing username)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// Password - Optional (only if changing password)
    /// </summary>
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    public string? Password { get; set; }

    /// <summary>
    /// User full name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "User name cannot exceed 200 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// User nickname - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Nickname cannot exceed 200 characters")]
    public string? NickName { get; set; }

    /// <summary>
    /// User email address - Optional (empty string allowed for updates)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }

    /// <summary>
    /// Notification recipient name - Optional
    /// </summary>
    [MaxLength(200, ErrorMessage = "Notification name cannot exceed 200 characters")]
    public string? NotificationName { get; set; }

    /// <summary>
    /// Notification email address - Optional (empty string allowed for updates)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Notification email cannot exceed 200 characters")]
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Indicates if user is a supervisor - Optional
    /// </summary>
    public bool? Supervisor { get; set; }

    /// <summary>
    /// Menu access level - Optional
    /// </summary>
    [MaxLength(20, ErrorMessage = "Menu level cannot exceed 20 characters")]
    public string? MenuLevel { get; set; }

    /// <summary>
    /// User operation type - Optional
    /// </summary>
    [MaxLength(50, ErrorMessage = "Operation cannot exceed 50 characters")]
    public string? Operation { get; set; }

    /// <summary>
    /// Code - Can be Office Code or Warehouse Code - Optional
    /// </summary>
    [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
    public string? Code { get; set; }
}
