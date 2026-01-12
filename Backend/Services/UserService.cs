// Author: Hassan
// Date: 2025-11-25
// Description: Service for User management - handles business logic (active users + all users)

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for User service operations
/// </summary>
public interface IUserService
{
    Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
    Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersIncludingInactiveAsync();
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid userId);
}

/// <summary>
/// Service implementation for User management
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IAuthService authService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active users
    /// </summary>
    public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllActiveAsync();
            var userDtos = users.Select(MapToDto);

            return ApiResponse<IEnumerable<UserDto>>.SuccessResponse(
                userDtos,
                "Users retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return ApiResponse<IEnumerable<UserDto>>.ErrorResponse(
                "Failed to retrieve users",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get all users (active and inactive)
    /// </summary>
    public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersIncludingInactiveAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(MapToDto);

            return ApiResponse<IEnumerable<UserDto>>.SuccessResponse(
                userDtos,
                "All users retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return ApiResponse<IEnumerable<UserDto>>.ErrorResponse(
                "Failed to retrieve all users",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResponse(
                    "User not found",
                    $"User with ID '{userId}' does not exist"
                );
            }

            return ApiResponse<UserDto>.SuccessResponse(
                MapToDto(user),
                "User retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user: {UserId}", userId);
            return ApiResponse<UserDto>.ErrorResponse(
                "Failed to retrieve user",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Hash the password using the same method as AuthService
            var passwordHash = _authService.HashPassword(request.Password);

            var user = new UserMaster
            {
                // UserId will be auto-generated as Guid
                Username = request.Username,
                PasswordHash = passwordHash,
                Name = request.Name ?? request.Username, // Use Name if provided, otherwise use Username
                Email = request.Email,
                Role = request.MenuLevel ?? "Scanner", // Default role based on MenuLevel
                MenuLevel = request.MenuLevel ?? "Scanner",
                Operation = request.Operation,
                LocationId = request.Code,
                Code = request.Code,
                IsSupervisor = request.Supervisor ?? false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("User created successfully: {UserId}", createdUser.UserId);

            return ApiResponse<UserDto>.SuccessResponse(
                MapToDto(createdUser),
                "User created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Name}", request.Name);
            return ApiResponse<UserDto>.ErrorResponse(
                "Failed to create user",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        try
        {
            // Convert empty strings to null for optional fields
            if (string.IsNullOrWhiteSpace(request.Email)) request.Email = null;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.ErrorResponse(
                    "User update failed",
                    $"User with ID '{userId}' does not exist"
                );
            }

            // Update user properties (UserId is read-only as per requirements)

            // Update username if provided
            if (!string.IsNullOrEmpty(request.Username))
            {
                user.Username = request.Username;
            }

            // Update name if provided
            if (!string.IsNullOrEmpty(request.Name))
            {
                user.Name = request.Name;
            }

            user.Email = request.Email;
            user.MenuLevel = request.MenuLevel ?? user.MenuLevel;
            user.Operation = request.Operation;
            user.LocationId = request.Code;
            user.Code = request.Code;
            user.IsSupervisor = request.Supervisor ?? user.IsSupervisor;

            // Update role based on MenuLevel if provided
            if (!string.IsNullOrEmpty(request.MenuLevel))
            {
                user.Role = request.MenuLevel;
            }

            // Update password only if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = _authService.HashPassword(request.Password);
            }

            user.UpdatedAt = DateTime.Now;

            var updatedUser = await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User updated successfully: {UserId}", userId);

            return ApiResponse<UserDto>.SuccessResponse(
                MapToDto(updatedUser),
                "User updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return ApiResponse<UserDto>.ErrorResponse(
                "Failed to update user",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "User deletion failed",
                    $"User with ID '{userId}' does not exist"
                );
            }

            var deleted = await _userRepository.DeleteAsync(userId);

            if (deleted)
            {
                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "User deleted successfully"
                );
            }

            return ApiResponse<bool>.ErrorResponse(
                "User deletion failed",
                "Failed to delete user"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete user",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Map UserMaster entity to UserDto (excluding password hash)
    /// </summary>
    private static UserDto MapToDto(UserMaster user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Name = user.Name,
            NickName = user.Name, // Using Name as NickName if not separately stored
            Email = user.Email,
            NotificationName = null, // Not stored in current entity
            NotificationEmail = null, // Not stored in current entity
            Supervisor = user.IsSupervisor,
            MenuLevel = user.MenuLevel,
            Operation = user.Operation,
            Code = user.Code,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
