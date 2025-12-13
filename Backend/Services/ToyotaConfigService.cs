// Author: Hassan
// Date: 2025-12-14
// Description: Service for Toyota API Configuration management - handles business logic and OAuth token testing

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Services;

/// <summary>
/// Interface for Toyota API Configuration service operations
/// </summary>
public interface IToyotaConfigService
{
    Task<ApiResponse<List<ToyotaConfigResponseDto>>> GetAllConfigsAsync();
    Task<ApiResponse<ToyotaConfigResponseDto>> GetConfigByIdAsync(Guid configId);
    Task<ApiResponse<ToyotaConfigResponseDto>> GetActiveConfigByEnvironmentAsync(string environment);
    Task<ApiResponse<ToyotaConfigResponseDto>> CreateConfigAsync(ToyotaConfigCreateDto dto, string userId);
    Task<ApiResponse<ToyotaConfigResponseDto>> UpdateConfigAsync(Guid configId, ToyotaConfigUpdateDto dto, string userId);
    Task<ApiResponse<bool>> DeleteConfigAsync(Guid configId);
    Task<ApiResponse<ToyotaConnectionTestDto>> TestConnectionAsync(Guid configId);
}

/// <summary>
/// Service implementation for Toyota API Configuration management
/// </summary>
public class ToyotaConfigService : IToyotaConfigService
{
    private readonly IToyotaConfigRepository _repository;
    private readonly ILogger<ToyotaConfigService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ToyotaConfigService(
        IToyotaConfigRepository repository,
        ILogger<ToyotaConfigService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Get all Toyota API configurations
    /// </summary>
    public async Task<ApiResponse<List<ToyotaConfigResponseDto>>> GetAllConfigsAsync()
    {
        try
        {
            var configs = await _repository.GetAllConfigsAsync();
            var dtos = configs.Select(MapToResponseDto).ToList();

            return ApiResponse<List<ToyotaConfigResponseDto>>.SuccessResponse(
                dtos,
                $"Retrieved {dtos.Count} Toyota API configuration(s)"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Toyota API configurations");
            return ApiResponse<List<ToyotaConfigResponseDto>>.ErrorResponse(
                "Failed to retrieve Toyota API configurations",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get Toyota API configuration by ID
    /// </summary>
    public async Task<ApiResponse<ToyotaConfigResponseDto>> GetConfigByIdAsync(Guid configId)
    {
        try
        {
            var config = await _repository.GetConfigByIdAsync(configId);

            if (config == null)
            {
                return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                    "Toyota API configuration not found",
                    $"No configuration found with ID {configId}"
                );
            }

            return ApiResponse<ToyotaConfigResponseDto>.SuccessResponse(
                MapToResponseDto(config),
                "Toyota API configuration retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Toyota API configuration {ConfigId}", configId);
            return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                "Failed to retrieve Toyota API configuration",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get active Toyota API configuration by environment
    /// </summary>
    public async Task<ApiResponse<ToyotaConfigResponseDto>> GetActiveConfigByEnvironmentAsync(string environment)
    {
        try
        {
            var config = await _repository.GetActiveConfigByEnvironmentAsync(environment);

            if (config == null)
            {
                return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                    "Active Toyota API configuration not found",
                    $"No active configuration found for environment: {environment}"
                );
            }

            return ApiResponse<ToyotaConfigResponseDto>.SuccessResponse(
                MapToResponseDto(config),
                $"Active Toyota API configuration for {environment} retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active Toyota API configuration for {Environment}", environment);
            return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                "Failed to retrieve active Toyota API configuration",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Create new Toyota API configuration
    /// </summary>
    public async Task<ApiResponse<ToyotaConfigResponseDto>> CreateConfigAsync(ToyotaConfigCreateDto dto, string userId)
    {
        try
        {
            // Validate environment
            if (!IsValidEnvironment(dto.Environment))
            {
                return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                    "Invalid environment",
                    "Environment must be 'QA' or 'PROD'"
                );
            }

            var config = new ToyotaApiConfig
            {
                Environment = dto.Environment.ToUpper(),
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret, // In production, encrypt this
                TokenUrl = dto.TokenUrl,
                ApiBaseUrl = dto.ApiBaseUrl,
                IsActive = dto.IsActive,
                ApplicationName = dto.ApplicationName,
                CreatedBy = userId
            };

            var created = await _repository.CreateConfigAsync(config);

            _logger.LogInformation("Toyota API configuration created for {Environment} by user {UserId}",
                created.Environment, userId);

            return ApiResponse<ToyotaConfigResponseDto>.SuccessResponse(
                MapToResponseDto(created),
                "Toyota API configuration created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Toyota API configuration");
            return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                "Failed to create Toyota API configuration",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update existing Toyota API configuration
    /// </summary>
    public async Task<ApiResponse<ToyotaConfigResponseDto>> UpdateConfigAsync(
        Guid configId,
        ToyotaConfigUpdateDto dto,
        string userId)
    {
        try
        {
            var existing = await _repository.GetConfigByIdAsync(configId);

            if (existing == null)
            {
                return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                    "Toyota API configuration not found",
                    $"No configuration found with ID {configId}"
                );
            }

            // Update only non-null fields
            if (!string.IsNullOrWhiteSpace(dto.Environment))
            {
                if (!IsValidEnvironment(dto.Environment))
                {
                    return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                        "Invalid environment",
                        "Environment must be 'QA' or 'PROD'"
                    );
                }
                existing.Environment = dto.Environment.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(dto.ClientId))
                existing.ClientId = dto.ClientId;

            if (!string.IsNullOrWhiteSpace(dto.ClientSecret))
                existing.ClientSecret = dto.ClientSecret; // In production, encrypt this

            if (!string.IsNullOrWhiteSpace(dto.TokenUrl))
                existing.TokenUrl = dto.TokenUrl;

            if (!string.IsNullOrWhiteSpace(dto.ApiBaseUrl))
                existing.ApiBaseUrl = dto.ApiBaseUrl;

            if (dto.IsActive.HasValue)
                existing.IsActive = dto.IsActive.Value;

            if (dto.ApplicationName != null)
                existing.ApplicationName = dto.ApplicationName;

            existing.UpdatedBy = userId;

            var updated = await _repository.UpdateConfigAsync(existing);

            _logger.LogInformation("Toyota API configuration {ConfigId} updated by user {UserId}",
                configId, userId);

            return ApiResponse<ToyotaConfigResponseDto>.SuccessResponse(
                MapToResponseDto(updated),
                "Toyota API configuration updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Toyota API configuration {ConfigId}", configId);
            return ApiResponse<ToyotaConfigResponseDto>.ErrorResponse(
                "Failed to update Toyota API configuration",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Delete Toyota API configuration
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteConfigAsync(Guid configId)
    {
        try
        {
            var deleted = await _repository.DeleteConfigAsync(configId);

            if (!deleted)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Toyota API configuration not found",
                    $"No configuration found with ID {configId}"
                );
            }

            _logger.LogInformation("Toyota API configuration {ConfigId} deleted", configId);

            return ApiResponse<bool>.SuccessResponse(
                true,
                "Toyota API configuration deleted successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Toyota API configuration {ConfigId}", configId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete Toyota API configuration",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Test Toyota API connection by attempting to acquire OAuth token
    /// </summary>
    public async Task<ApiResponse<ToyotaConnectionTestDto>> TestConnectionAsync(Guid configId)
    {
        try
        {
            var config = await _repository.GetConfigByIdAsync(configId);

            if (config == null)
            {
                return ApiResponse<ToyotaConnectionTestDto>.ErrorResponse(
                    "Toyota API configuration not found",
                    $"No configuration found with ID {configId}"
                );
            }

            _logger.LogInformation("Testing Toyota API connection for config {ConfigId} ({Environment})",
                configId, config.Environment);

            // Attempt to get OAuth token
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Toyota API spec (V2.0, page 6) specifies only these OAuth parameters:
            // client_id, client_secret, grant_type (NO resource or scope parameter)
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.ClientId },
                { "client_secret", config.ClientSecret }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, config.TokenUrl)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            _logger.LogInformation("Sending OAuth token request to {TokenUrl} with client_id {ClientId}",
                config.TokenUrl, config.ClientId);

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("OAuth token response - Status: {StatusCode}, Content: {Content}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent);

                if (tokenResponse?.AccessToken != null)
                {
                    var tokenPreview = tokenResponse.AccessToken.Length > 20
                        ? tokenResponse.AccessToken.Substring(0, 20) + "..."
                        : tokenResponse.AccessToken;

                    // Parse expires_in from string to int (Microsoft returns it as string)
                    int? expiresIn = null;
                    if (!string.IsNullOrEmpty(tokenResponse.ExpiresIn) && int.TryParse(tokenResponse.ExpiresIn, out var exp))
                    {
                        expiresIn = exp;
                    }

                    // Calculate expiry time using expires_on Unix timestamp if available
                    DateTime? expiresAt = null;
                    string expiryDetails = "";

                    if (!string.IsNullOrEmpty(tokenResponse.ExpiresOn) && long.TryParse(tokenResponse.ExpiresOn, out var expiresOnUnix))
                    {
                        expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresOnUnix).UtcDateTime;
                        expiryDetails = $"Token expires at: {expiresAt:yyyy-MM-dd HH:mm:ss} UTC (Unix: {expiresOnUnix})";
                        _logger.LogInformation("Token expires_on: {ExpiresOn} UTC", expiresAt);
                    }
                    else if (expiresIn.HasValue)
                    {
                        expiresAt = DateTime.UtcNow.AddSeconds(expiresIn.Value);
                        expiryDetails = $"Token expires in {expiresIn.Value} seconds (approximately {expiresAt:yyyy-MM-dd HH:mm:ss} UTC)";
                    }

                    // Parse not_before if available
                    if (!string.IsNullOrEmpty(tokenResponse.NotBefore) && long.TryParse(tokenResponse.NotBefore, out var notBeforeUnix))
                    {
                        var notBefore = DateTimeOffset.FromUnixTimeSeconds(notBeforeUnix).UtcDateTime;
                        expiryDetails += $" | Valid from: {notBefore:yyyy-MM-dd HH:mm:ss} UTC";
                    }

                    var message = $"Successfully connected to Toyota API and obtained OAuth token. {expiryDetails}";

                    var testResult = new ToyotaConnectionTestDto
                    {
                        Success = true,
                        Message = message,
                        TokenPreview = tokenPreview,
                        ExpiresIn = expiresIn,
                        TestedAt = DateTime.UtcNow
                    };

                    _logger.LogInformation("Toyota API connection test successful for config {ConfigId}. {Details}",
                        configId, expiryDetails);

                    return ApiResponse<ToyotaConnectionTestDto>.SuccessResponse(
                        testResult,
                        "Connection test successful"
                    );
                }
            }

            // Connection failed
            var failureResult = new ToyotaConnectionTestDto
            {
                Success = false,
                Message = $"Failed to obtain OAuth token. Status: {response.StatusCode}. Response: {responseContent}",
                TestedAt = DateTime.UtcNow
            };

            _logger.LogWarning("Toyota API connection test failed for config {ConfigId}. Status: {StatusCode}, Response: {Response}",
                configId, response.StatusCode, responseContent);

            return ApiResponse<ToyotaConnectionTestDto>.SuccessResponse(
                failureResult,
                "Connection test completed with failure"
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error testing Toyota API connection for config {ConfigId}", configId);

            var errorResult = new ToyotaConnectionTestDto
            {
                Success = false,
                Message = $"Connection test failed - HTTP error: {ex.Message}. Check TokenUrl and network connectivity.",
                TestedAt = DateTime.UtcNow
            };

            return ApiResponse<ToyotaConnectionTestDto>.SuccessResponse(
                errorResult,
                "Connection test completed with error"
            );
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout testing Toyota API connection for config {ConfigId}", configId);

            var errorResult = new ToyotaConnectionTestDto
            {
                Success = false,
                Message = $"Connection test timed out after 30 seconds. Check TokenUrl and network connectivity.",
                TestedAt = DateTime.UtcNow
            };

            return ApiResponse<ToyotaConnectionTestDto>.SuccessResponse(
                errorResult,
                "Connection test timed out"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error testing Toyota API connection for config {ConfigId}", configId);

            var errorResult = new ToyotaConnectionTestDto
            {
                Success = false,
                Message = $"Connection test failed with exception: {ex.Message}",
                TestedAt = DateTime.UtcNow
            };

            return ApiResponse<ToyotaConnectionTestDto>.SuccessResponse(
                errorResult,
                "Connection test failed"
            );
        }
    }

    #region Helper Methods

    /// <summary>
    /// Map ToyotaApiConfig entity to response DTO (masks ClientSecret)
    /// </summary>
    private static ToyotaConfigResponseDto MapToResponseDto(ToyotaApiConfig config)
    {
        return new ToyotaConfigResponseDto
        {
            ConfigId = config.ConfigId,
            Environment = config.Environment,
            ClientId = config.ClientId,
            ClientSecretMasked = "********", // Never expose the real secret
            TokenUrl = config.TokenUrl,
            ApiBaseUrl = config.ApiBaseUrl,
            IsActive = config.IsActive,
            ApplicationName = config.ApplicationName,
            CreatedBy = config.CreatedBy,
            CreatedAt = config.CreatedAt,
            UpdatedBy = config.UpdatedBy,
            UpdatedAt = config.UpdatedAt
        };
    }

    /// <summary>
    /// Validate environment value
    /// </summary>
    private static bool IsValidEnvironment(string environment)
    {
        var validEnvironments = new[] { "QA", "PROD", "DEV" };
        return validEnvironments.Contains(environment.ToUpper());
    }

    #endregion

    #region OAuth Token Response Model

    /// <summary>
    /// OAuth token response from Microsoft Identity Platform
    /// Includes all fields for complete token information
    /// </summary>
    private class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string? ExpiresIn { get; set; }  // Microsoft returns this as string "3599" (seconds)

        [JsonPropertyName("ext_expires_in")]
        public string? ExtExpiresIn { get; set; }  // Extended expiration time in seconds

        [JsonPropertyName("expires_on")]
        public string? ExpiresOn { get; set; }  // Unix timestamp when token expires

        [JsonPropertyName("not_before")]
        public string? NotBefore { get; set; }  // Unix timestamp when token becomes valid
    }

    #endregion
}
