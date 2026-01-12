// Author: Hassan
// Date: 2025-11-11
// Description: Authentication service with JWT token generation and password hashing

using Backend.Configuration;
using Backend.Models;
using Backend.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for authentication operations
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<SessionValidationResponse> ValidateSessionAsync(string token);
        string HashPassword(string password);
    }

    /// <summary>
    /// Authentication service implementation
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtConfiguration _jwtConfig;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            IOptions<JwtConfiguration> jwtConfig,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("========================================");
            _logger.LogInformation("[LOGIN START] CorrelationId: {CorrelationId}", correlationId);
            _logger.LogInformation("Username: {Username}", request.Username);
            _logger.LogInformation("Timestamp: {Timestamp}", DateTime.UtcNow);

            try
            {
                _logger.LogDebug("[STEP 1] Hashing password for username: {Username}", request.Username);
                // Hash the password (in production, compare with stored hash)
                var passwordHash = HashPassword(request.Password);
                _logger.LogDebug("[STEP 1 COMPLETE] Password hashed successfully");
                _logger.LogDebug("Password hash (first 10 chars): {HashPrefix}...", passwordHash.Substring(0, Math.Min(10, passwordHash.Length)));

                _logger.LogDebug("[STEP 2] Calling database repository for user authentication");
                _logger.LogDebug("Username: {Username}", request.Username);

                // Call stored procedure to validate credentials
                var loginResult = await _authRepository.LoginAsync(request.Username, passwordHash);

                _logger.LogDebug("[STEP 2 COMPLETE] Database query completed");
                _logger.LogDebug("Login result: Success={Success}, Username={Username}, Role={Role}",
                    loginResult?.Success, loginResult?.Username, loginResult?.Role);

                if (loginResult == null || loginResult.Success == 0)
                {
                    _logger.LogWarning("[LOGIN FAILED] Authentication failed for username: {Username}", request.Username);
                    _logger.LogWarning("Reason: {ErrorMessage}", loginResult?.ErrorMessage ?? "Invalid username or password");
                    _logger.LogWarning("CorrelationId: {CorrelationId}", correlationId);
                    _logger.LogInformation("========================================");

                    return new LoginResponse
                    {
                        Success = false,
                        Error = loginResult?.ErrorMessage ?? "Invalid username or password"
                    };
                }

                _logger.LogDebug("[STEP 3] Determining token expiry based on role");
                // Determine token expiry based on role
                var expiryMinutes = loginResult.Role?.ToUpper() == "SUPERVISOR"
                    ? _jwtConfig.SupervisorExpiryMinutes
                    : _jwtConfig.ExpiryMinutes;

                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
                _logger.LogDebug("[STEP 3 COMPLETE] Role: {Role}, Expiry: {ExpiryMinutes} minutes, ExpiresAt: {ExpiresAt}",
                    loginResult.Role, expiryMinutes, expiresAt);

                _logger.LogDebug("[STEP 4] Generating JWT token");
                // Generate JWT token
                var token = GenerateJwtToken(loginResult, expiresAt);
                _logger.LogDebug("[STEP 4 COMPLETE] JWT token generated successfully");
                _logger.LogDebug("Token (first 20 chars): {TokenPrefix}...", token.Substring(0, Math.Min(20, token.Length)));

                _logger.LogDebug("[STEP 5] Creating session record in database");
                _logger.LogDebug("UserId: {UserId}, ExpiresAt: {ExpiresAt}", loginResult.Id, expiresAt);

                // Create session record
                await _authRepository.CreateSessionAsync(loginResult.Id!.Value, token, expiresAt);

                _logger.LogDebug("[STEP 5 COMPLETE] Session record created successfully");

                _logger.LogInformation("[LOGIN SUCCESS] User authenticated successfully");
                _logger.LogInformation("Username: {Username}", loginResult.Username);
                _logger.LogInformation("UserId: {UserId}", loginResult.Id);
                _logger.LogInformation("Role: {Role}", loginResult.Role);
                _logger.LogInformation("LocationId: {LocationId}", loginResult.LocationId);
                _logger.LogInformation("Token expiry: {ExpiresAt}", expiresAt);
                _logger.LogInformation("CorrelationId: {CorrelationId}", correlationId);
                _logger.LogInformation("========================================");

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserInfo
                    {
                        Id = loginResult.Id ?? Guid.Empty,
                        Username = loginResult.Username ?? string.Empty,
                        Name = loginResult.Name ?? string.Empty,
                        Role = loginResult.Role ?? string.Empty,
                        LocationId = loginResult.LocationId ?? string.Empty,
                        Supervisor = loginResult.IsSupervisor
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LOGIN ERROR] Exception occurred during login");
                _logger.LogError("Username: {Username}", request.Username);
                _logger.LogError("Exception type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("Exception message: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                _logger.LogError("CorrelationId: {CorrelationId}", correlationId);
                _logger.LogInformation("========================================");
                throw;
            }
        }

        /// <summary>
        /// Validate JWT token and session
        /// </summary>
        public async Task<SessionValidationResponse> ValidateSessionAsync(string token)
        {
            try
            {
                // Validate token format and signature
                var principal = ValidateJwtToken(token);
                if (principal == null)
                {
                    return new SessionValidationResponse
                    {
                        Valid = false,
                        Error = "Invalid token format"
                    };
                }

                // Validate session in database
                var validationResult = await _authRepository.ValidateSessionAsync(token);

                if (validationResult == null || validationResult.Valid == 0)
                {
                    return new SessionValidationResponse
                    {
                        Valid = false,
                        Error = validationResult?.ErrorMessage ?? "Session expired"
                    };
                }

                return new SessionValidationResponse
                {
                    Valid = true,
                    User = new UserInfo
                    {
                        Id = validationResult.Id ?? Guid.Empty,
                        Username = validationResult.Username ?? string.Empty,
                        Name = validationResult.Name ?? string.Empty,
                        Role = validationResult.Role ?? string.Empty,
                        LocationId = validationResult.LocationId ?? string.Empty,
                        Supervisor = validationResult.IsSupervisor
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session validation");
                return new SessionValidationResponse
                {
                    Valid = false,
                    Error = "Session validation failed"
                };
            }
        }

        /// <summary>
        /// Generate JWT token with user claims
        /// </summary>
        private string GenerateJwtToken(LoginResult user, DateTime expiresAt)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                new Claim("LocationId", user.LocationId ?? string.Empty),
                new Claim("IsSupervisor", user.IsSupervisor.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token and return ClaimsPrincipal
        /// </summary>
        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtConfig.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hash password using SHA256 (Note: Use bcrypt/argon2 in production)
        /// </summary>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
