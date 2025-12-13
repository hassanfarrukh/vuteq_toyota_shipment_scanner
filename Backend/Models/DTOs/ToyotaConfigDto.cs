// Author: Hassan
// Date: 2025-12-13
// Description: DTOs for Toyota API Configuration management

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Response DTO for Toyota API Config (masks ClientSecret)
/// </summary>
public class ToyotaConfigResponseDto
{
    public Guid ConfigId { get; set; }
    public string Environment { get; set; } = null!;
    public string? ApplicationName { get; set; }
    public string ClientId { get; set; } = null!;
    public string ClientSecretMasked { get; set; } = "********"; // Never expose real secret
    public string TokenUrl { get; set; } = null!;
    public string ApiBaseUrl { get; set; } = null!;
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating Toyota API Config
/// </summary>
public class ToyotaConfigCreateDto
{
    [Required(ErrorMessage = "Environment is required (QA or PROD)")]
    [MaxLength(20, ErrorMessage = "Environment cannot exceed 20 characters")]
    public string Environment { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "Application Name cannot exceed 200 characters")]
    public string? ApplicationName { get; set; }

    [Required(ErrorMessage = "Client ID is required")]
    [MaxLength(100, ErrorMessage = "Client ID cannot exceed 100 characters")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "Client Secret is required")]
    [MaxLength(500, ErrorMessage = "Client Secret cannot exceed 500 characters")]
    public string ClientSecret { get; set; } = null!;

    [Required(ErrorMessage = "Token URL is required")]
    [MaxLength(500, ErrorMessage = "Token URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Token URL must be a valid URL")]
    public string TokenUrl { get; set; } = null!;

    [Required(ErrorMessage = "API Base URL is required")]
    [MaxLength(500, ErrorMessage = "API Base URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "API Base URL must be a valid URL")]
    public string ApiBaseUrl { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for updating Toyota API Config
/// </summary>
public class ToyotaConfigUpdateDto
{
    [MaxLength(20, ErrorMessage = "Environment cannot exceed 20 characters")]
    public string? Environment { get; set; }

    [MaxLength(200, ErrorMessage = "Application Name cannot exceed 200 characters")]
    public string? ApplicationName { get; set; }

    [MaxLength(100, ErrorMessage = "Client ID cannot exceed 100 characters")]
    public string? ClientId { get; set; }

    [MaxLength(500, ErrorMessage = "Client Secret cannot exceed 500 characters")]
    public string? ClientSecret { get; set; } // Only update if provided (not null/empty)

    [MaxLength(500, ErrorMessage = "Token URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Token URL must be a valid URL")]
    public string? TokenUrl { get; set; }

    [MaxLength(500, ErrorMessage = "API Base URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "API Base URL must be a valid URL")]
    public string? ApiBaseUrl { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// Response for Toyota API connection test
/// </summary>
public class ToyotaConnectionTestDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? TokenPreview { get; set; } // First 20 chars of token if successful
    public int? ExpiresIn { get; set; }
    public DateTime? TestedAt { get; set; }
}
