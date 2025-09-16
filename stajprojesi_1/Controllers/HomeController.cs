using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using stajprojesi_1.Models;
using System.Diagnostics;
using System.Text.Json;
using Serilog;

namespace stajprojesi_1.Controllers
{
    public class HomeController : BaseController
    {
        private string connectionString = "Server=.;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly Serilog.ILogger _logger;

        public HomeController(IConfiguration configuration, Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var rolId = HttpContext.Session.GetInt32("RolId");
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            _logger.Information("🏠 ANA SAYFA ERİŞİMİ BAŞLADI: 👤 Kullanıcı: {User} | 🔑 Rol ID: {RolId} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | ⏰ Zaman: {Timestamp}",
                kullaniciAdi ?? "Anonim", rolId?.ToString() ?? "Belirsiz", ipAddress, userAgent, timestamp);
            
            if (string.IsNullOrEmpty(kullaniciAdi) || rolId == null)
            {
                _logger.Warning("⚠️ YETKİSİZ ANA SAYFA ERİŞİMİ: 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 📍 Yönlendirme: Login sayfası | ⏰ Zaman: {Timestamp}",
                    ipAddress, userAgent, timestamp);
                return RedirectToAction("Login", "Account");
            }

            _logger.Information("✅ KULLANICI YETKİSİ DOĞRULANDI: 👤 Kullanıcı: {User} | 🔑 Rol ID: {RolId} | 🌍 IP: {IP} | 📍 Ana sayfa yükleniyor | ⏰ Zaman: {Timestamp}",
                kullaniciAdi, rolId, ipAddress, timestamp);

            try
            {
                _logger.Information("📊 ANA SAYFA REFERANS VERİLERİ YÜKLENİYOR: 👤 Kullanıcı: {User} | 🔑 Rol ID: {RolId} | 🌍 IP: {IP}",
                    kullaniciAdi, rolId, ipAddress);
                
                // BasvuranBirim listesi
                var basvuranBirimListesi = new List<SelectListItem>();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, ReferansAdi FROM Referanslar WHERE ReferansTuru = 'BasvuranBirim' AND Silindi = 0 ORDER BY ReferansAdi";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                basvuranBirimListesi.Add(new SelectListItem
                                {
                                    Value = reader["Id"].ToString(),
                                    Text = reader["ReferansAdi"].ToString()
                                });
                            }
                        }
                    }
                }
                ViewBag.BasvuranBirimListesi = basvuranBirimListesi;

                // BasvuruYapilanProje listesi
                var basvuruYapilanProjeListesi = new List<SelectListItem>();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, ReferansAdi FROM Referanslar WHERE ReferansTuru = 'BasvuruYapilanProje' AND Silindi = 0 ORDER BY ReferansAdi";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                basvuruYapilanProjeListesi.Add(new SelectListItem
                                {
                                    Value = reader["Id"].ToString(),
                                    Text = reader["ReferansAdi"].ToString()
                                });
                            }
                        }
                    }
                }
                ViewBag.BasvuruYapilanProjeListesi = basvuruYapilanProjeListesi;

                _logger.Information("✅ ANA SAYFA REFERANS VERİLERİ BAŞARILI: 👤 Kullanıcı: {User} | 🔑 Rol ID: {RolId} | 🌍 IP: {IP} | 📊 BasvuranBirim: {BasvuranBirimCount} | 📊 BasvuruYapilanProje: {BasvuruYapilanProjeCount}",
                    kullaniciAdi, rolId, ipAddress, basvuranBirimListesi.Count, basvuruYapilanProjeListesi.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "💥 ANA SAYFA REFERANS VERİLERİ HATASI: 👤 Kullanıcı: {User} | 🔑 Rol ID: {RolId} | 🌍 IP: {IP} | ❌ Hata: {ErrorMessage}",
                    kullaniciAdi, rolId, ipAddress, ex.Message);
                
                // Fallback data
                ViewBag.BasvuranBirimListesi = new List<SelectListItem>();
                ViewBag.BasvuruYapilanProjeListesi = new List<SelectListItem>();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            _logger.Information("🔒 PRIVACY SAYFASI ERİŞİMİ: 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 📍 Yol: GET /Home/Privacy | ⏰ Zaman: {Timestamp}",
                kullaniciAdi ?? "Anonim", ipAddress, userAgent, timestamp);
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            
            _logger.Error("🚨 HATA SAYFASI ERİŞİMİ: 👤 Kullanıcı: {User} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent} | 🆔 Request ID: {RequestId} | 📍 Yol: GET /Home/Error | ⏰ Zaman: {Timestamp}",
                kullaniciAdi ?? "Anonim", ipAddress, userAgent, requestId, timestamp);
            
            return View(new ErrorViewModel { RequestId = requestId });
        }

        public IActionResult TestReferans()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.Information("🧪 TEST REFERANS METODU ÇAĞRILDI: 👤 Kullanıcı: {User}", kullaniciAdi ?? "Anonim");
            return Content("Test referans çalışıyor!");
        }

        [HttpPost]
        public IActionResult BasvuruKaydet(BasvuruModel model)
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.Information("📝 BAŞVURU KAYDETME İŞLEMİ BAŞLADI: 👤 Kullanıcı: {User}", kullaniciAdi ?? "Anonim");
            _logger.Information("📋 Proje Adı: {ProjeAdi}", model.Proje_adi);
            _logger.Information("🏢 Başvuran Birim: {Birim}", model.Basvuran_birim);
            _logger.Information("🎯 Başvuru Yapılan Proje: {Proje}", model.Basvuru_yapilan_proje);
            
            try
            {
                // Başvuru kaydetme işlemi burada yapılacak
                _logger.Information("✅ BAŞVURU BAŞARILI KAYDEDİLDİ: 👤 Kullanıcı: {User}", kullaniciAdi ?? "Anonim");
                return Json(new { success = true, message = "Başvuru kaydedildi!" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ BAŞVURU KAYDETME HATASI: 👤 Kullanıcı: {User} | ❌ Hata: {ErrorMessage}",
                    kullaniciAdi ?? "Anonim", ex.Message);
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }
    }
}    

