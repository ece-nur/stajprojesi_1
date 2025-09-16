using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using stajprojesi_1.Services;
using System;

namespace stajprojesi_1.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<AccountController> _logger;


        public AccountController(
            IConfiguration configuration,
            ILoggingService loggingService,
            ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _loggingService = loggingService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogInformation("🔐 LOGIN SAYFASI ERİŞİMİ: 🌍 IP: {IP} | 🔍 UA: {UA} | ⏰ {TS}",
                ipAddress, userAgent, timestamp);

            _loggingService.LogUserAction("Login Sayfası Erişimi", "GET /Account/Login", null, ipAddress);
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogInformation("🔐 GİRİŞ DENEMESİ BAŞLADI: 📧 {Email} | 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                email, ipAddress, userAgent, timestamp);

            _loggingService.LogUserAction("Giriş Denemesi", $"Email: {email}, User Agent: {userAgent}", null, ipAddress);

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("DefaultConnection bağlantı dizesi bulunamadı.");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    const string query = @"
                        SELECT u.Id, u.Username, u.Email, u.Password, u.RolId
                        FROM Users u
                        WHERE ((u.Email = @Email AND u.IsDeleted = 0)
                               OR (u.Username = @Email AND u.IsDeleted = 0))
                          AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Password", password ?? string.Empty);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                int rolId = Convert.ToInt32(reader["RolId"]);
                                string userId = reader["Id"].ToString();
                                string emailFromDb = reader["Email"].ToString();

                                _logger.LogInformation("✅ BAŞARILI GİRİŞ: 👤 {User} | 🆔 {UserId} | Rol: {RolId} | 📧 {Email} | 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                                    username, userId, rolId, emailFromDb, ipAddress, userAgent, timestamp);

                                _loggingService.LogLogin(username, true, ipAddress);
                                _loggingService.LogUserAction("Başarılı Giriş", $"Rol ID: {rolId}, Email: {emailFromDb}", username, ipAddress);

                                HttpContext.Session.SetString("KullaniciAdi", username);
                                HttpContext.Session.SetInt32("RolId", rolId);

                                _logger.LogInformation("🔑 SESSION OLUŞTURULDU: 👤 {User} | Rol: {RolId} | 🌍 {IP}",
                                    username, rolId, ipAddress);

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }

                // Kullanıcı bulunamadı -> başarısız giriş
                _logger.LogWarning("❌ BAŞARISIZ GİRİŞ: 📧 {Email} | 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                    email, ipAddress, userAgent, timestamp);

                _loggingService.LogLogin(email, false, ipAddress, "Email/Kullanıcı adı veya şifre hatalı");
                _loggingService.LogSecurityEvent("Başarısız Giriş Denemesi", $"Email: {email}, IP: {ipAddress}", null, ipAddress);

                ViewBag.Error = "Email/Kullanıcı adı ya da şifre hatalı.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 GİRİŞ SİSTEMİ HATASI: 📧 {Email} | 🌍 {IP} | 🔍 {UA} | ❌ {Err} | ⏰ {TS}",
                    email, ipAddress, userAgent, ex.Message, timestamp);

                _loggingService.LogError("Giriş Hatası", ex.Message, ex, null);
                _loggingService.LogSecurityEvent("Giriş Sistemi Hatası", $"Email: {email}, Hata: {ex.Message}", null, ipAddress);

                ViewBag.Error = "Hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogInformation("🚪 ÇIKIŞ İŞLEMİ BAŞLADI: 👤 {User} | 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                kullaniciAdi, ipAddress, userAgent, timestamp);

            if (!string.IsNullOrEmpty(kullaniciAdi))
            {
                _loggingService.LogLogout(kullaniciAdi, ipAddress);
                _loggingService.LogUserAction("Çıkış Yapıldı", "GET /Account/Logout", kullaniciAdi, ipAddress);

                HttpContext.Session.Clear();

                _logger.LogInformation("✅ ÇIKIŞ BAŞARILI: 👤 {User} | 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                    kullaniciAdi, ipAddress, userAgent, timestamp);
            }
            else
            {
                _logger.LogWarning("⚠️ ÇIKIŞ - SESSION YOK: 🌍 {IP} | 🔍 {UA} | ⏰ {TS}",
                    ipAddress, userAgent, timestamp);
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            var user = HttpContext.Session.GetString("KullaniciAdi") ?? "Anonim";
            _logger.LogWarning("🚫 ERİŞİM REDDEDİLDİ! 👤 {User} | 🌐 {IP} | ⏰ {Time}",
                user,
                HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            return View();
        }
    }
}
