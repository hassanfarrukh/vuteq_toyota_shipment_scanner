// Author: Hassan
// Date: 2025-11-11
// Description: Authentication controller with login and session validation endpoints

using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    /// <summary>
    /// Authentication and session management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User authentication endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Login response with JWT token and user information</returns>
        /// <response code="200">Login successful - returns user info and JWT token</response>
        /// <response code="401">Invalid credentials - authentication failed</response>
        /// <response code="400">Bad request - validation errors</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("========================================");
            _logger.LogInformation("[AUTH CONTROLLER] Incoming login request");
            _logger.LogInformation("RequestId: {RequestId}", requestId);
            _logger.LogInformation("Endpoint: POST /api/Auth/login");
            _logger.LogInformation("Client IP: {ClientIP}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("User-Agent: {UserAgent}", Request.Headers["User-Agent"].ToString());
            _logger.LogInformation("Timestamp: {Timestamp}", DateTime.Now);
            _logger.LogInformation("Request body - Username: {Username}", request?.Username ?? "NULL");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[AUTH CONTROLLER] Model validation failed");
                _logger.LogWarning("RequestId: {RequestId}", requestId);
                _logger.LogWarning("Validation errors: {@ValidationErrors}", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogInformation("Returning 400 Bad Request");
                _logger.LogInformation("========================================");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("[AUTH CONTROLLER] Model validation passed, calling AuthService");
            _logger.LogInformation("Username: {Username}", request?.Username ?? "UNKNOWN");

            if (request == null)
            {
                _logger.LogError("[AUTH CONTROLLER] Request is null after validation");
                return BadRequest("Invalid request");
            }

            var response = await _authService.LoginAsync(request);

            if (!response.Success)
            {
                _logger.LogWarning("[AUTH CONTROLLER] AuthService returned failure");
                _logger.LogWarning("RequestId: {RequestId}", requestId);
                _logger.LogWarning("Username: {Username}", request.Username);
                _logger.LogWarning("Error: {Error}", response.Error);
                _logger.LogInformation("Returning 401 Unauthorized");
                _logger.LogInformation("Response body: {@Response}", new { success = response.Success, error = response.Error });
                _logger.LogInformation("========================================");
                return Unauthorized(response);
            }

            _logger.LogInformation("[AUTH CONTROLLER] AuthService returned success");
            _logger.LogInformation("RequestId: {RequestId}", requestId);
            _logger.LogInformation("Username: {Username}", request.Username);
            _logger.LogInformation("UserId: {UserId}", response.User?.Id);
            _logger.LogInformation("Role: {Role}", response.User?.Role);
            _logger.LogInformation("Token generated: {TokenPrefix}...", response.Token?.Substring(0, Math.Min(20, response.Token?.Length ?? 0)));
            _logger.LogInformation("Returning 200 OK");
            _logger.LogInformation("Response body: {@Response}", new
            {
                success = response.Success,
                user = new
                {
                    id = response.User?.Id,
                    username = response.User?.Username,
                    name = response.User?.Name,
                    role = response.User?.Role,
                    locationId = response.User?.LocationId
                },
                tokenLength = response.Token?.Length
            });
            _logger.LogInformation("========================================");

            return Ok(response);
        }

        /// <summary>
        /// Validate stored session token
        /// </summary>
        /// <returns>Session validation response with user information if valid</returns>
        /// <response code="200">Session is valid - returns user information</response>
        /// <response code="401">Session is invalid or expired</response>
        [HttpGet("v1/auth/session/validate")]
        [Authorize]
        [ProducesResponseType(typeof(SessionValidationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SessionValidationResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateSession()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Session validation failed - missing or invalid Authorization header");
                return Unauthorized(new SessionValidationResponse
                {
                    Valid = false,
                    Error = "Missing or invalid authorization header"
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            _logger.LogInformation("Validating session token");

            var response = await _authService.ValidateSessionAsync(token);

            if (!response.Valid)
            {
                _logger.LogWarning("Session validation failed");
                return Unauthorized(response);
            }

            _logger.LogInformation("Session validation successful for user: {Username}", response.User?.Username);
            return Ok(response);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.Now });
        }
    }
}
