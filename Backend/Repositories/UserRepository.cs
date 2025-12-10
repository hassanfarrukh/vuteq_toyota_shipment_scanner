// Author: Hassan
// Date: 2025-11-25
// Description: Repository for User entity - handles data access using EF Core (active users + all users)

using Backend.Data;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

/// <summary>
/// Interface for User repository operations
/// </summary>
public interface IUserRepository
{
    Task<IEnumerable<UserMaster>> GetAllActiveAsync();
    Task<IEnumerable<UserMaster>> GetAllAsync();
    Task<UserMaster?> GetByIdAsync(Guid userId);
    Task<UserMaster?> GetByUsernameAsync(string username);
    Task<UserMaster> CreateAsync(UserMaster user);
    Task<UserMaster> UpdateAsync(UserMaster user);
    Task<bool> DeleteAsync(Guid userId);
}

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly VuteqDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(VuteqDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active users
    /// </summary>
    public async Task<IEnumerable<UserMaster>> GetAllActiveAsync()
    {
        try
        {
            return await _context.UserMasters
                .Where(u => u.IsActive)
                .OrderBy(u => u.UserId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            throw;
        }
    }

    /// <summary>
    /// Get all users (active and inactive)
    /// </summary>
    public async Task<IEnumerable<UserMaster>> GetAllAsync()
    {
        try
        {
            return await _context.UserMasters
                .OrderBy(u => u.UserId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserMaster?> GetByIdAsync(Guid userId)
    {
        try
        {
            return await _context.UserMasters
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<UserMaster?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _context.UserMasters
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<UserMaster> CreateAsync(UserMaster user)
    {
        try
        {
            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {UserId}", user.UserId);
            throw;
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task<UserMaster> UpdateAsync(UserMaster user)
    {
        try
        {
            _context.UserMasters.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
            throw;
        }
    }

    /// <summary>
    /// Delete a user (soft delete by setting IsActive = false)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid userId)
    {
        try
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            throw;
        }
    }
}
