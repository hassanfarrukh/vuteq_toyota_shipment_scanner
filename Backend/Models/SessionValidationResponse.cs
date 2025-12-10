// Author: Hassan
// Date: 2025-11-11
// Description: Session validation response model

namespace Backend.Models
{
    /// <summary>
    /// Session validation response model
    /// </summary>
    public class SessionValidationResponse
    {
        /// <summary>
        /// Indicates if the session is valid
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// User information if session is valid
        /// </summary>
        public UserInfo? User { get; set; }

        /// <summary>
        /// Error message if session is invalid
        /// </summary>
        public string? Error { get; set; }
    }
}
