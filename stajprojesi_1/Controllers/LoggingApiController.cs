using Microsoft.AspNetCore.Mvc;
using stajprojesi_1.Services;
using Serilog;

namespace stajprojesi_1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingApiController : ControllerBase
    {
        private readonly ILoggingService _loggingService;
        private readonly Serilog.ILogger _logger;

        public LoggingApiController(ILoggingService loggingService, Serilog.ILogger logger)
        {
            _loggingService = loggingService;
            _logger = logger;
        }

        [HttpPost("frontend")]
        public IActionResult LogFrontendEvent([FromBody] FrontendLogRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                // Frontend log'ları backend'e entegre et
                switch (request.Category)
                {
                    case "error":
                        _loggingService.LogError("Frontend Hatası", request.Details?.ToString() ?? "Bilinmeyen hata", null, request.SessionId);
                        break;
                    
                    case "security":
                        _loggingService.LogSecurityEvent("Frontend Güvenlik Olayı", request.Details?.ToString() ?? "Bilinmeyen güvenlik olayı", null, ipAddress);
                        break;
                    
                    case "session_end":
                        _loggingService.LogUserAction("Frontend Session Sonu", request.Details?.ToString() ?? "Session sonlandı", null, ipAddress);
                        break;
                    
                    case "form_submit":
                        _loggingService.LogUserAction("Frontend Form Gönderimi", request.Details?.ToString() ?? "Form gönderildi", null, ipAddress);
                        break;
                    
                    default:
                        _loggingService.LogUserAction("Frontend Olay", request.Details?.ToString() ?? "Frontend olayı", null, ipAddress);
                        break;
                }

                // Detaylı log bilgisi
                _logger.Information("🔍 FRONTEND LOG: {Category} | {Action} | Session: {SessionId} | IP: {IP} | User Agent: {UserAgent} | Details: {Details}",
                    request.Category, request.Action, request.SessionId, ipAddress, userAgent, request.Details);

                return Ok(new { success = true, message = "Log kaydedildi" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Frontend log kaydetme hatası");
                return StatusCode(500, new { success = false, error = "Log kaydedilemedi" });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "Logging API",
                version = "1.0.0"
            });
        }
    }

    public class FrontendLogRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public object Details { get; set; } = new();
        public string UserAgent { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public int InteractionCount { get; set; }
    }
}
