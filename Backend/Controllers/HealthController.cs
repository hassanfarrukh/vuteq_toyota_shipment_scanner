// Author: Hassan
// Date: 2025-11-11
// Description: Health check endpoint for Docker and monitoring systems

using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring and Docker health checks
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested");
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "VUTEQ Scanner API",
                version = "1.0.0"
            });
        }
    }
}
