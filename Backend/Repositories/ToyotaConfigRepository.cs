// Author: Hassan
// Date: 2025-12-13
// Description: Repository for Toyota API Configuration - handles data access using EF Core

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for Toyota API Configuration repository operations
/// </summary>
public interface IToyotaConfigRepository
{
    Task<List<ToyotaApiConfig>> GetAllConfigsAsync();
    Task<ToyotaApiConfig?> GetConfigByIdAsync(Guid configId);
    Task<ToyotaApiConfig?> GetActiveConfigByEnvironmentAsync(string environment);
    Task<ToyotaApiConfig> CreateConfigAsync(ToyotaApiConfig config);
    Task<ToyotaApiConfig> UpdateConfigAsync(ToyotaApiConfig config);
    Task<bool> DeleteConfigAsync(Guid configId);
    Task<bool> ConfigExistsAsync(string environment);
}

/// <summary>
/// Repository implementation for Toyota API Configuration
/// </summary>
public class ToyotaConfigRepository : IToyotaConfigRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<ToyotaConfigRepository> _logger;

    public ToyotaConfigRepository(VuteqDbContext context, ILogger<ToyotaConfigRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all Toyota API configurations
    /// </summary>
    public async Task<List<ToyotaApiConfig>> GetAllConfigsAsync()
    {
        try
        {
            return await _context.Set<ToyotaApiConfig>()
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all Toyota API configurations");
            throw;
        }
    }

    /// <summary>
    /// Get Toyota API configuration by ID
    /// </summary>
    public async Task<ToyotaApiConfig?> GetConfigByIdAsync(Guid configId)
    {
        try
        {
            return await _context.Set<ToyotaApiConfig>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConfigId == configId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Toyota API configuration with ID {ConfigId}", configId);
            throw;
        }
    }

    /// <summary>
    /// Get active Toyota API configuration by environment (QA or PROD)
    /// </summary>
    public async Task<ToyotaApiConfig?> GetActiveConfigByEnvironmentAsync(string environment)
    {
        try
        {
            return await _context.Set<ToyotaApiConfig>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Environment == environment && c.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active Toyota API configuration for environment {Environment}", environment);
            throw;
        }
    }

    /// <summary>
    /// Create new Toyota API configuration
    /// </summary>
    public async Task<ToyotaApiConfig> CreateConfigAsync(ToyotaApiConfig config)
    {
        try
        {
            config.CreatedAt = DateTime.Now;
            _context.Set<ToyotaApiConfig>().Add(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toyota API configuration created for environment {Environment}", config.Environment);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Toyota API configuration");
            throw;
        }
    }

    /// <summary>
    /// Update existing Toyota API configuration
    /// </summary>
    public async Task<ToyotaApiConfig> UpdateConfigAsync(ToyotaApiConfig config)
    {
        try
        {
            config.UpdatedAt = DateTime.Now;
            _context.Set<ToyotaApiConfig>().Update(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toyota API configuration {ConfigId} updated", config.ConfigId);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Toyota API configuration {ConfigId}", config.ConfigId);
            throw;
        }
    }

    /// <summary>
    /// Delete Toyota API configuration
    /// </summary>
    public async Task<bool> DeleteConfigAsync(Guid configId)
    {
        try
        {
            var config = await _context.Set<ToyotaApiConfig>()
                .FirstOrDefaultAsync(c => c.ConfigId == configId);

            if (config == null)
            {
                return false;
            }

            _context.Set<ToyotaApiConfig>().Remove(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toyota API configuration {ConfigId} deleted", configId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Toyota API configuration {ConfigId}", configId);
            throw;
        }
    }

    /// <summary>
    /// Check if a configuration exists for a specific environment
    /// </summary>
    public async Task<bool> ConfigExistsAsync(string environment)
    {
        try
        {
            return await _context.Set<ToyotaApiConfig>()
                .AnyAsync(c => c.Environment == environment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if Toyota API configuration exists for environment {Environment}", environment);
            throw;
        }
    }
}
