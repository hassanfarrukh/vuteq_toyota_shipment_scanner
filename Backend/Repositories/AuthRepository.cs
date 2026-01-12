// Author: Hassan
// Date: 2025-11-24 (Updated to use EF Core LINQ queries)
// Description: Authentication repository for database operations using Entity Framework Core

using Backend.Data;
using Backend.Models;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    /// <summary>
    /// Repository interface for authentication operations
    /// </summary>
    public interface IAuthRepository
    {
        Task<LoginResult?> LoginAsync(string username, string passwordHash);
        Task<SessionValidationResult?> ValidateSessionAsync(string token);
        Task<string> CreateSessionAsync(Guid userId, string token, DateTime expiresAt);
    }

    /// <summary>
    /// Authentication repository implementation using Entity Framework Core
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly VuteqDbContext _context;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(VuteqDbContext context, ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user using EF Core LINQ queries
        /// </summary>
        public async Task<LoginResult?> LoginAsync(string username, string passwordHash)
        {
            _logger.LogDebug("[REPOSITORY] LoginAsync called");
            _logger.LogDebug("Username: {Username}", username);
            _logger.LogDebug("Password hash (first 10 chars): {HashPrefix}...", passwordHash.Substring(0, Math.Min(10, passwordHash.Length)));

            try
            {
                _logger.LogDebug("[DATABASE QUERY] Querying tblUserMaster for username: {Username}", username);
                _logger.LogDebug("Query: SELECT * FROM tblUserMaster WHERE Username = @Username AND IsActive = 1");

                // 1. Find active user by username
                var user = await _context.UserMasters
                    .Where(u => u.Username == username && u.IsActive)
                    .FirstOrDefaultAsync();

                _logger.LogDebug("[DATABASE QUERY COMPLETE] User found: {UserFound}", user != null);

                // 2. Validate user exists and password matches
                if (user == null)
                {
                    _logger.LogWarning("[REPOSITORY] User not found or inactive: {Username}", username);
                    return new LoginResult
                    {
                        Success = 0,
                        ErrorMessage = "Invalid username or password"
                    };
                }

                _logger.LogDebug("User found - UserId: {UserId}, Username: {Username}, Role: {Role}", user.UserId, user.Username, user.Role);
                _logger.LogDebug("=== FULL PASSWORD HASH COMPARISON ===");
                _logger.LogDebug("Stored password hash (FULL):   {StoredHash}", user.PasswordHash ?? "NULL");
                _logger.LogDebug("Provided password hash (FULL): {ProvidedHash}", passwordHash);
                _logger.LogDebug("Hash lengths - Stored: {StoredLength}, Provided: {ProvidedLength}",
                    user.PasswordHash?.Length ?? 0, passwordHash.Length);
                _logger.LogDebug("Hashes match: {HashesMatch}", user.PasswordHash == passwordHash);

                if (user.PasswordHash != passwordHash)
                {
                    _logger.LogWarning("[REPOSITORY] Password mismatch for username: {Username}", username);
                    _logger.LogWarning("FULL COMPARISON FAILED - See debug logs above for details");
                    return new LoginResult
                    {
                        Success = 0,
                        ErrorMessage = "Invalid username or password"
                    };
                }

                _logger.LogDebug("[REPOSITORY] Password validation successful");
                _logger.LogDebug("[DATABASE UPDATE] Updating LastLoginAt for user: {UserId}", user.UserId);

                // 3. Update user's last login timestamp
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogDebug("[DATABASE UPDATE COMPLETE] LastLoginAt updated to: {LastLoginAt}", user.LastLoginAt);

                // 4. Return successful login result
                _logger.LogInformation("[REPOSITORY] Login successful for user: {Username} (UserId: {UserId})", username, user.UserId);

                return new LoginResult
                {
                    Success = 1,
                    Id = user.UserId,
                    Username = user.Username,
                    Name = user.Name,
                    Role = user.Role,
                    LocationId = user.LocationId,
                    IsSupervisor = user.IsSupervisor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPOSITORY ERROR] Exception during login");
                _logger.LogError("Username: {Username}", username);
                _logger.LogError("Exception type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("Exception message: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Validate session token using EF Core LINQ queries
        /// </summary>
        public async Task<SessionValidationResult?> ValidateSessionAsync(string token)
        {
            try
            {
                // 1. Find active session with user details
                var session = await _context.UserSessions
                    .Include(s => s.User)
                    .Where(s => s.Token == token && s.IsActive)
                    .FirstOrDefaultAsync();

                // 2. Check if session exists
                if (session == null)
                {
                    _logger.LogWarning("Session validation failed: Token not found or inactive");
                    return new SessionValidationResult
                    {
                        Valid = 0,
                        ErrorMessage = "Invalid or expired session"
                    };
                }

                // 3. Check if session expired
                if (session.ExpiresAt < DateTime.Now)
                {
                    _logger.LogWarning("Session validation failed: Token expired for user {UserId}", session.UserId);

                    // Mark session as inactive
                    session.IsActive = false;
                    await _context.SaveChangesAsync();

                    return new SessionValidationResult
                    {
                        Valid = 0,
                        ErrorMessage = "Session expired"
                    };
                }

                // 4. Update last activity timestamp
                session.LastActivityAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // 5. Return successful validation result
                return new SessionValidationResult
                {
                    Valid = 1,
                    Id = session.User.UserId,
                    Username = session.User.Username,
                    Name = session.User.Name,
                    Role = session.User.Role,
                    LocationId = session.User.LocationId,
                    IsSupervisor = session.User.IsSupervisor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session validation");
                throw;
            }
        }

        /// <summary>
        /// Create a new session record in tblUserSessions using EF Core
        /// </summary>
        public async Task<string> CreateSessionAsync(Guid userId, string token, DateTime expiresAt)
        {
            try
            {
                // 1. Optional: Expire all previous active sessions for this user (uncomment if needed)
                // var existingSessions = await _context.UserSessions
                //     .Where(s => s.UserId == userId && s.IsActive)
                //     .ToListAsync();
                // existingSessions.ForEach(s => s.IsActive = false);

                // 2. Create new session entity
                var session = new UserSession
                {
                    SessionId = Guid.NewGuid(),
                    UserId = userId,
                    Token = token,
                    ExpiresAt = expiresAt,
                    IsActive = true,
                    LastActivityAt = DateTime.Now
                };

                // 3. Add to context and save
                _context.UserSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Session created successfully for user: {UserId}", userId);
                return session.SessionId.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
                throw;
            }
        }
    }

    /// <summary>
    /// Login result DTO
    /// </summary>
    public class LoginResult
    {
        public int Success { get; set; }
        public Guid? Id { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? LocationId { get; set; }
        public bool IsSupervisor { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Session validation result DTO
    /// </summary>
    public class SessionValidationResult
    {
        public int Valid { get; set; }
        public Guid? Id { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? LocationId { get; set; }
        public bool IsSupervisor { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
