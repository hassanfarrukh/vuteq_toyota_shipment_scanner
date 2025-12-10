// Author: Hassan
// Date: 2025-11-11
// Description: JWT configuration settings class

namespace Backend.Configuration
{
    /// <summary>
    /// JWT authentication configuration settings
    /// </summary>
    public class JwtConfiguration
    {
        /// <summary>
        /// Secret key for JWT token generation
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Token issuer (API identifier)
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Token audience (client application identifier)
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Token expiry time in minutes for regular users
        /// </summary>
        public int ExpiryMinutes { get; set; } = 480; // 8 hours

        /// <summary>
        /// Token expiry time in minutes for supervisors
        /// </summary>
        public int SupervisorExpiryMinutes { get; set; } = 720; // 12 hours
    }
}
