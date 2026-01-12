// Author: Hassan
// Date: 2025-11-11
// Description: Login response model with user details and JWT token

namespace Backend.Models
{
    /// <summary>
    /// Login response model
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Indicates if login was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// JWT token for authenticated requests
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// User information
        /// </summary>
        public UserInfo? User { get; set; }

        /// <summary>
        /// Error message if login failed
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// User information returned in login response
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's full name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User role (ADMIN, SUPERVISOR, OPERATOR)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Location/Warehouse ID
        /// </summary>
        public string LocationId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the user is a supervisor
        /// </summary>
        public bool Supervisor { get; set; }
    }
}
