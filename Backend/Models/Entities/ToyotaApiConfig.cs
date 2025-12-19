// Author: Hassan
// Date: 2025-12-13
// Description: Entity for Toyota API configuration (OAuth credentials and endpoints)

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities;

/// <summary>
/// Toyota API Configuration for QA and Production environments
/// Stores OAuth2 credentials and API endpoints
/// </summary>
[Table("tblToyotaApiConfig")]
public class ToyotaApiConfig : AuditableEntity
{
    [Key]
    public Guid ConfigId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Environment: QA or PROD
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Environment { get; set; } = null!;

    /// <summary>
    /// Application name for identification
    /// </summary>
    [MaxLength(200)]
    public string? ApplicationName { get; set; }

    /// <summary>
    /// OAuth2 Client ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// OAuth2 Client Secret (should be encrypted in production)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// OAuth2 Token URL
    /// QA: https://login.microsoftonline.com/tmnatest.onmicrosoft.com/oauth2/token
    /// PROD: https://login.microsoftonline.com/toyota1.onmicrosoft.com/oauth2/token
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string TokenUrl { get; set; } = null!;

    /// <summary>
    /// API Base URL
    /// QA: https://api.dev.scs.toyota.com/spbapi/rest/
    /// PROD: https://api.scs.toyota.com/spbapi/rest/
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ApiBaseUrl { get; set; } = null!;

    /// <summary>
    /// OAuth2 Resource URL (V2.1)
    /// QA: https://tmnatest.onmicrosoft.com/supplier-order-information-oauth-lios-product
    /// PROD: https://toyota1.onmicrosoft.com/supplier-order-information-oauth-lios-product
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ResourceUrl { get; set; } = null!;

    /// <summary>
    /// x-client-id header value for API requests (V2.1)
    /// Example: a1012aed-c89a-49d3-a796-63a4345ecc98
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string XClientId { get; set; } = null!;

    /// <summary>
    /// Whether this configuration is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
