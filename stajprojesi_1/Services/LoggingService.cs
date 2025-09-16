using Serilog;
using Microsoft.AspNetCore.Http;

namespace stajprojesi_1.Services
{
    public interface ILoggingService
    {
        void LogUserAction(string action, string details, string? userId = null, string? ipAddress = null);
        void LogLogin(string username, bool success, string? ipAddress = null, string? reason = null);
        void LogLogout(string username, string? ipAddress = null);
        void LogDataOperation(string operation, string entityType, string entityId, string? userId = null, bool success = true, string? details = null);
        void LogSystemEvent(string eventType, string details, string severity = "Information");
        void LogError(string errorType, string message, Exception? exception = null, string? userId = null);
        void LogSecurityEvent(string eventType, string details, string? userId = null, string? ipAddress = null);
    }

    public class LoggingService : ILoggingService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Serilog.ILogger _logger;

        public LoggingService(IHttpContextAccessor httpContextAccessor, Serilog.ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void LogUserAction(string action, string details, string? userId = null, string? ipAddress = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var user = userId ?? context?.Session?.GetString("KullaniciAdi") ?? "Anonim";
            var ip = ipAddress ?? context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var path = context?.Request?.Path.Value ?? "Unknown";
            var method = context?.Request?.Method ?? "Unknown";

            _logger.Information("👤 KULLANICI İŞLEMİ: {Action} | 👤 Kullanıcı: {User} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 📝 Detay: {Details}",
                action, user, method, path, ip, userAgent, details);
        }

        public void LogLogin(string username, bool success, string? ipAddress = null, string? reason = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var ip = ipAddress ?? context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (success)
            {
                _logger.Information("🔐 BAŞARILI GİRİŞ: 👤 Kullanıcı: {Username} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | ⏰ Zaman: {Timestamp}",
                    username, ip, userAgent, timestamp);
            }
            else
            {
                _logger.Warning("❌ BAŞARISIZ GİRİŞ: 👤 Kullanıcı: {Username} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 📝 Sebep: {Reason} | ⏰ Zaman: {Timestamp}",
                    username, ip, userAgent, reason ?? "Bilinmeyen", timestamp);
            }
        }

        public void LogLogout(string username, string? ipAddress = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var ip = ipAddress ?? context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.Information("🚪 ÇIKIŞ: 👤 Kullanıcı: {Username} | 🌍 IP: {IP} | ⏰ Zaman: {Timestamp}",
                username, ip, timestamp);
        }

        public void LogDataOperation(string operation, string entityType, string entityId, string? userId = null, bool success = true, string? details = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var user = userId ?? context?.Session?.GetString("KullaniciAdi") ?? "Anonim";
            var ip = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            if (success)
            {
                _logger.Information("💾 VERİ İŞLEMİ: {Operation} | 🏷️ Tür: {EntityType} | 🆔 ID: {EntityId} | 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 📝 Detay: {Details}",
                    operation, entityType, entityId, user, ip, details ?? "Başarılı");
            }
            else
            {
                _logger.Warning("⚠️ VERİ İŞLEMİ HATASI: {Operation} | 🏷️ Tür: {EntityType} | 🆔 ID: {EntityId} | 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 📝 Detay: {Details}",
                    operation, entityType, entityId, user, ip, details ?? "Başarısız");
            }
        }

        public void LogSystemEvent(string eventType, string details, string severity = "Information")
        {
            switch (severity.ToLower())
            {
                case "error":
                    _logger.Error("🚨 SİSTEM HATASI: {EventType} | 📝 Detay: {Details}", eventType, details);
                    break;
                case "warning":
                    _logger.Warning("⚠️ SİSTEM UYARISI: {EventType} | 📝 Detay: {Details}", eventType, details);
                    break;
                case "debug":
                    _logger.Debug("🔍 SİSTEM DEBUG: {EventType} | 📝 Detay: {Details}", eventType, details);
                    break;
                default:
                    _logger.Information("ℹ️ SİSTEM OLAYI: {EventType} | 📝 Detay: {Details}", eventType, details);
                    break;
            }
        }

        public void LogError(string errorType, string message, Exception? exception = null, string? userId = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var user = userId ?? context?.Session?.GetString("KullaniciAdi") ?? "Anonim";
            var ip = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var path = context?.Request?.Path.Value ?? "Unknown";

            if (exception != null)
            {
                _logger.Error(exception, "💥 HATA: {ErrorType} | 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 📍 Yol: {Path} | 📝 Mesaj: {Message}",
                    errorType, user, ip, path, message);
            }
            else
            {
                _logger.Error("💥 HATA: {ErrorType} | 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 📍 Yol: {Path} | 📝 Mesaj: {Message}",
                    errorType, user, ip, path, message);
            }
        }

        public void LogSecurityEvent(string eventType, string details, string? userId = null, string? ipAddress = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var user = userId ?? context?.Session?.GetString("KullaniciAdi") ?? "Anonim";
            var ip = ipAddress ?? context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.Warning("🔒 GÜVENLİK OLAYI: {EventType} | 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 📝 Detay: {Details} | ⏰ Zaman: {Timestamp}",
                eventType, user, ip, userAgent, details, timestamp);
        }
    }
}
