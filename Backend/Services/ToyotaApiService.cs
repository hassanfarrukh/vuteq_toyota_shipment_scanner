// Author: Hassan
// Date: 2025-12-14
// Description: Service for Toyota SCS API integration (OAuth + Skid Build + Shipment Load)

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Interface for Toyota SCS API Integration Service
/// </summary>
public interface IToyotaApiService
{
    Task<string?> GetAccessTokenAsync(string environment);
    Task<ToyotaApiResponse> SubmitSkidBuildAsync(string environment, List<ToyotaSkidBuildRequest> requests);
    Task<ToyotaApiResponse> SubmitShipmentLoadAsync(string environment, ToyotaShipmentLoadRequest request);
    (string TokenPreview, DateTime ExpiresAt, bool IsValid)? GetCachedTokenInfo(string environment);
}

/// <summary>
/// Toyota SCS API Integration Service
/// Handles OAuth2 authentication and API calls for Skid Build and Shipment Load
/// </summary>
public class ToyotaApiService : IToyotaApiService
{
    private readonly VuteqDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ToyotaApiService> _logger;

    // Cache tokens to avoid repeated auth calls
    private static readonly Dictionary<string, (string Token, DateTime ExpiresAt)> _tokenCache = new();
    private static readonly object _cacheLock = new();

    public ToyotaApiService(
        VuteqDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<ToyotaApiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get cached token information for debugging/monitoring purposes
    /// </summary>
    /// <param name="environment">Environment name (Dev, QA, Prod)</param>
    /// <returns>Cached token info or null if no cached token exists</returns>
    public (string TokenPreview, DateTime ExpiresAt, bool IsValid)? GetCachedTokenInfo(string environment)
    {
        lock (_cacheLock)
        {
            if (_tokenCache.TryGetValue(environment, out var cached))
            {
                var tokenPreview = cached.Token.Length > 20
                    ? cached.Token.Substring(0, 20) + "..."
                    : cached.Token;

                var isValid = cached.ExpiresAt > DateTime.UtcNow.AddMinutes(5);

                return (tokenPreview, cached.ExpiresAt, isValid);
            }
        }

        return null;
    }

    /// <summary>
    /// Get OAuth2 access token from Microsoft for Toyota API
    /// Tokens are cached to avoid repeated authentication calls
    /// </summary>
    /// <param name="environment">Environment name (Dev, QA, Prod)</param>
    /// <returns>Access token or null if authentication fails</returns>
    public async Task<string?> GetAccessTokenAsync(string environment)
    {
        // Check cache first (thread-safe)
        lock (_cacheLock)
        {
            if (_tokenCache.TryGetValue(environment, out var cached) && cached.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("Using cached Toyota API token for environment: {Environment}", environment);
                return cached.Token;
            }
        }

        // Fetch config from database
        var config = await _context.ToyotaApiConfigs
            .FirstOrDefaultAsync(c => c.Environment == environment && c.IsActive);

        if (config == null)
        {
            _logger.LogError("No active Toyota API config found for environment: {Environment}", environment);
            return null;
        }

        try
        {
            _logger.LogInformation("Requesting new OAuth2 token for Toyota API - Environment: {Environment}", environment);

            // Toyota API spec (V2.0, page 6) OAuth parameters: client_id, client_secret, grant_type only
            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret
            });

            var response = await client.PostAsync(config.TokenUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Toyota OAuth token - Status: {Status}, Body: {Body}",
                    response.StatusCode, responseBody);
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();

            // Microsoft returns expires_in as string "3599" instead of int 3599
            var expiresInString = tokenResponse.GetProperty("expires_in").GetString();
            var expiresIn = int.TryParse(expiresInString, out var exp) ? exp : 3600; // Default to 1 hour

            // Calculate expiry time - prefer expires_on Unix timestamp if available, otherwise use expires_in
            DateTime expiresAt;
            if (tokenResponse.TryGetProperty("expires_on", out var expiresOnElement))
            {
                var expiresOnString = expiresOnElement.GetString();
                if (!string.IsNullOrEmpty(expiresOnString) && long.TryParse(expiresOnString, out var expiresOnUnix))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresOnUnix).UtcDateTime;
                    _logger.LogInformation("Using expires_on Unix timestamp: {ExpiresOn} (UTC: {ExpiresAtUtc})",
                        expiresOnUnix, expiresAt);
                }
                else
                {
                    expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
                    _logger.LogInformation("expires_on parsing failed, using expires_in calculation");
                }
            }
            else
            {
                expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
                _logger.LogInformation("expires_on not found, using expires_in calculation");
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                // Cache the token (thread-safe)
                lock (_cacheLock)
                {
                    _tokenCache[environment] = (accessToken, expiresAt);
                }

                _logger.LogInformation("Successfully obtained Toyota API token - Expires at: {ExpiresAt} UTC (in {ExpiresIn} seconds)",
                    expiresAt, expiresIn);
                return accessToken;
            }

            _logger.LogError("Access token was null or empty in response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Toyota access token for environment: {Environment}", environment);
            return null;
        }
    }

    /// <summary>
    /// Submit Skid Build to Toyota API
    /// POST {baseUrl}/skid
    /// </summary>
    /// <param name="environment">Environment name (Dev, QA, Prod)</param>
    /// <param name="requests">List of skid build requests (one per order)</param>
    /// <returns>Toyota API response with confirmation number or error</returns>
    public async Task<ToyotaApiResponse> SubmitSkidBuildAsync(string environment, List<ToyotaSkidBuildRequest> requests)
    {
        try
        {
            _logger.LogInformation("Submitting Skid Build to Toyota API - Environment: {Environment}, Orders: {OrderCount}",
                environment, requests.Count);

            // Get access token
            var token = await GetAccessTokenAsync(environment);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to get access token for Skid Build submission");
                return new ToyotaApiResponse
                {
                    Code = 401,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Failed to authenticate with Toyota API" } }
                    }
                };
            }

            // Get API config for base URL
            var config = await _context.ToyotaApiConfigs
                .FirstOrDefaultAsync(c => c.Environment == environment && c.IsActive);

            if (config == null)
            {
                _logger.LogError("No active Toyota API config found for environment: {Environment}", environment);
                return new ToyotaApiResponse
                {
                    Code = 500,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Toyota API configuration not found" } }
                    }
                };
            }

            // Prepare HTTP request
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var jsonBody = JsonSerializer.Serialize(requests, jsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var endpoint = $"{config.ApiBaseUrl.TrimEnd('/')}/skid";
            _logger.LogInformation("Posting to Toyota Skid Build API: {Endpoint}", endpoint);
            _logger.LogInformation("Toyota Skid Build Payload: {Payload}", jsonBody);

            // Send request
            var response = await client.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Toyota API Response - Status: {Status}, Body: {Body}",
                response.StatusCode, responseBody);

            // Parse response
            var apiResponse = JsonSerializer.Deserialize<ToyotaApiResponse>(responseBody, jsonOptions);

            if (apiResponse == null)
            {
                return new ToyotaApiResponse
                {
                    Code = (int)response.StatusCode,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Failed to parse Toyota API response" } }
                    }
                };
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting Skid Build to Toyota API");
            return new ToyotaApiResponse
            {
                Code = 500,
                Messages = new List<ToyotaApiMessage>
                {
                    new() { Type = "Error", Message = new List<string> { $"Internal error: {ex.Message}" } }
                }
            };
        }
    }

    /// <summary>
    /// Submit Shipment Load to Toyota API
    /// POST {baseUrl}/trailer
    /// </summary>
    /// <param name="environment">Environment name (Dev, QA, Prod)</param>
    /// <param name="request">Shipment load request with trailer and orders</param>
    /// <returns>Toyota API response with confirmation number or error</returns>
    public async Task<ToyotaApiResponse> SubmitShipmentLoadAsync(string environment, ToyotaShipmentLoadRequest request)
    {
        try
        {
            _logger.LogInformation("Submitting Shipment Load to Toyota API - Environment: {Environment}, Trailer: {Trailer}, Orders: {OrderCount}",
                environment, request.TrailerNumber, request.Orders.Count);

            // Get access token
            var token = await GetAccessTokenAsync(environment);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to get access token for Shipment Load submission");
                return new ToyotaApiResponse
                {
                    Code = 401,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Failed to authenticate with Toyota API" } }
                    }
                };
            }

            // Get API config for base URL
            var config = await _context.ToyotaApiConfigs
                .FirstOrDefaultAsync(c => c.Environment == environment && c.IsActive);

            if (config == null)
            {
                _logger.LogError("No active Toyota API config found for environment: {Environment}", environment);
                return new ToyotaApiResponse
                {
                    Code = 500,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Toyota API configuration not found" } }
                    }
                };
            }

            // Prepare HTTP request
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var jsonBody = JsonSerializer.Serialize(request, jsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var endpoint = $"{config.ApiBaseUrl.TrimEnd('/')}/trailer";
            _logger.LogInformation("Posting to Toyota Shipment Load API: {Endpoint}", endpoint);
            _logger.LogInformation("Toyota Shipment Load Payload: {Payload}", jsonBody);

            // Send request
            var response = await client.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Toyota API Response - Status: {Status}, Body: {Body}",
                response.StatusCode, responseBody);

            // Parse response
            var apiResponse = JsonSerializer.Deserialize<ToyotaApiResponse>(responseBody, jsonOptions);

            if (apiResponse == null)
            {
                return new ToyotaApiResponse
                {
                    Code = (int)response.StatusCode,
                    Messages = new List<ToyotaApiMessage>
                    {
                        new() { Type = "Error", Message = new List<string> { "Failed to parse Toyota API response" } }
                    }
                };
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting Shipment Load to Toyota API");
            return new ToyotaApiResponse
            {
                Code = 500,
                Messages = new List<ToyotaApiMessage>
                {
                    new() { Type = "Error", Message = new List<string> { $"Internal error: {ex.Message}" } }
                }
            };
        }
    }
}

// ===== REQUEST/RESPONSE DTOs for Toyota API =====

/// <summary>
/// Toyota API Response Structure
/// </summary>
public class ToyotaApiResponse
{
    public int Code { get; set; }
    public List<ToyotaApiMessage>? Messages { get; set; }
    public string? ConfirmationNumber { get; set; }
    public List<ToyotaConfirmedOrder>? ConfirmedOrders { get; set; }
    public ToyotaConfirmedTrailer? ConfirmedTrailer { get; set; }

    /// <summary>
    /// Success if Code is 200 and no error messages
    /// </summary>
    public bool Success => Code == 200 && (Messages == null || Messages.Count == 0);

    /// <summary>
    /// First error message if any
    /// </summary>
    public string? ErrorMessage => Messages?.FirstOrDefault()?.Message?.FirstOrDefault();
}

/// <summary>
/// Toyota API Message (Error or Warning)
/// </summary>
public class ToyotaApiMessage
{
    public string? KeyObject { get; set; }
    public List<string>? Message { get; set; }
    public string? Type { get; set; }
}

/// <summary>
/// Toyota Confirmed Order (returned in Skid Build response)
/// </summary>
public class ToyotaConfirmedOrder
{
    public string Order { get; set; } = null!;
    public string Supplier { get; set; } = null!;
    public string Plant { get; set; } = null!;
    public string Dock { get; set; } = null!;
    public string ConfirmationNumber { get; set; } = null!;
}

/// <summary>
/// Toyota Confirmed Trailer (returned in Shipment Load response)
/// </summary>
public class ToyotaConfirmedTrailer
{
    public string Supplier { get; set; } = null!;
    public string Route { get; set; } = null!;
    public string Run { get; set; } = null!;
    public string TrailerNumber { get; set; } = null!;
    public string ConfirmationNumber { get; set; } = null!;
}

// ===== SKID BUILD REQUEST MODELS =====

/// <summary>
/// Skid Build Request structure (matches Toyota API spec)
/// One request per order
/// </summary>
public class ToyotaSkidBuildRequest
{
    public string Order { get; set; } = null!;
    public string Supplier { get; set; } = null!;
    public string Plant { get; set; } = null!;
    public string Dock { get; set; } = null!;
    public List<ToyotaException>? Exceptions { get; set; }
    public List<ToyotaSkid> Skids { get; set; } = new();
}

/// <summary>
/// Toyota Skid with Kanbans and RFID details
/// </summary>
public class ToyotaSkid
{
    public string Palletization { get; set; } = null!;
    public string SkidId { get; set; } = null!;
    public List<ToyotaKanbanItem> Kanbans { get; set; } = new();
    public List<ToyotaRfidDetail>? RfidDetails { get; set; }
}

/// <summary>
/// Toyota Kanban Item (part on a skid)
/// </summary>
public class ToyotaKanbanItem
{
    public string LineSideAddress { get; set; } = null!;
    public string PartNumber { get; set; } = null!;
    public string Kanban { get; set; } = null!;
    public int Qpc { get; set; }
    public int BoxNumber { get; set; }
    public string? ManifestNumber { get; set; }
    public string? RfId { get; set; }
    public bool KanbanCut { get; set; } = false;
}

// ===== SHIPMENT LOAD REQUEST MODELS =====

/// <summary>
/// Shipment Load Request structure (matches Toyota API spec)
/// Contains trailer info and all orders being loaded
/// </summary>
public class ToyotaShipmentLoadRequest
{
    public string Supplier { get; set; } = null!;
    public string Route { get; set; } = null!;
    public string Run { get; set; } = null!;
    public string TrailerNumber { get; set; } = null!;
    public bool DropHook { get; set; }
    public string? SealNumber { get; set; }
    public string? SupplierTeamFirstName { get; set; }
    public string? SupplierTeamLastName { get; set; }
    public string? LpCode { get; set; }
    public string? DriverTeamFirstName { get; set; }
    public string? DriverTeamLastName { get; set; }
    public List<ToyotaException>? Exceptions { get; set; }
    public List<ToyotaShipmentOrder> Orders { get; set; } = new();
}

/// <summary>
/// Order within a shipment load
/// </summary>
public class ToyotaShipmentOrder
{
    public string Order { get; set; } = null!;
    public string Supplier { get; set; } = null!;
    public string Plant { get; set; } = null!;
    public string Dock { get; set; } = null!;
    public string PickUp { get; set; } = null!; // Format: yyyy-MM-ddTHH:mm
    public List<ToyotaShipmentSkid> Skids { get; set; } = new();
}

/// <summary>
/// Skid reference in shipment load (minimal info)
/// </summary>
public class ToyotaShipmentSkid
{
    public string Palletization { get; set; } = null!;
    public string SkidId { get; set; } = null!;
    public List<ToyotaException>? Exceptions { get; set; }
    public bool SkidCut { get; set; } = false;
}

// ===== SHARED MODELS =====

/// <summary>
/// Toyota Exception (used in both Skid Build and Shipment Load)
/// </summary>
public class ToyotaException
{
    public string ExceptionCode { get; set; } = null!;
    public string? Comments { get; set; }
}

/// <summary>
/// RFID Detail (used in Skid Build)
/// </summary>
public class ToyotaRfidDetail
{
    public string? Rfid { get; set; }
    public string? Type { get; set; }
}
