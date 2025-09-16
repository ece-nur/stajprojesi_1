using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using stajprojesi_1.Models;


namespace stajprojesi_1.Controllers
{
    public class ReferansController:BaseController
    {
        private readonly string connectionString = "Server=.;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly ILogger<ReferansController> _logger;

        public ReferansController(ILogger<ReferansController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            List<string> tipler = new List<string>
        {
            "BasvuranBirim",
            "BasvuruYapilanProje", 
            "BasvuruYapilanTur",
            "KatilimciTuru",
            "BasvuruDonemi",
            "BasvuruDurumu"
        };

            ViewBag.Tipler = tipler;
            return View();
        }

        [HttpPost]
        public IActionResult Kaydet(string Tipi, string ReferansAdi)
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Referans kaydetme işlemi başlatıldı: {User}, Tür: {Tur}, Ad: {Ad}", 
                kullaniciAdi ?? "Anonim", Tipi, ReferansAdi);
            
            if (string.IsNullOrEmpty(Tipi) || string.IsNullOrEmpty(ReferansAdi))
            {
                _logger.LogWarning("Referans kaydetme işlemi eksik parametrelerle iptal edildi: {User}, Tür: {Tur}, Ad: {Ad}", 
                    kullaniciAdi, Tipi, ReferansAdi);
                return RedirectToAction("Ekle");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Referanslar (ReferansTuru, ReferansAdi, Silindi) VALUES (@ReferansTuru, @ReferansAdi, 0)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ReferansTuru", NormalizeKey(Tipi));
                    cmd.Parameters.AddWithValue("@ReferansAdi", ReferansAdi);
                    cmd.ExecuteNonQuery();
                }

                _logger.LogInformation("Referans başarıyla kaydedildi: {User}, Tür: {Tur}, Ad: {Ad}", 
                    kullaniciAdi, Tipi, ReferansAdi);
                return RedirectToAction("Ekle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans kaydetme hatası: {User}, Tür: {Tur}, Ad: {Ad}", 
                    kullaniciAdi, Tipi, ReferansAdi);
                throw;
            }
        }

        [HttpGet]
        public IActionResult Liste()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Referans listesi sayfası erişimi: {User}", kullaniciAdi ?? "Anonim");

            // Session kontrolü
            if (string.IsNullOrEmpty(kullaniciAdi) || 
                HttpContext.Session.GetInt32("RolId") == null)
            {
                _logger.LogWarning("Yetkisiz referans listesi erişimi, login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Referans türlerini ViewBag'e ekle
                List<string> tipler = new List<string>
                {
                    "BasvuranBirim",
                    "BasvuruYapilanProje", 
                    "BasvuruYapilanTur",
                    "KatilimciTuru",
                    "BasvuruDonemi",
                    "BasvuruDurumu"
                };

                ViewBag.ReferansTurleri = tipler.Select(t => new SelectListItem 
                { 
                    Value = t, 
                    Text = GetTurkishName(t) 
                }).ToList();

                _logger.LogInformation("Referans listesi sayfası başarıyla yüklendi: {User}, Tür sayısı: {Count}", 
                    kullaniciAdi, tipler.Count);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans listesi sayfası yüklenirken hata oluştu: {User}", kullaniciAdi);
                throw;
            }
        }

        [HttpGet]
        public JsonResult GetReferanslar(string referansTuru)
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Referans verileri getiriliyor: {User}, Tür: {Tur}", 
                kullaniciAdi ?? "Anonim", referansTuru);
            
            try
            {
                List<object> referanslar = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Id, ReferansAdi FROM Referanslar WHERE ReferansTuru = @tur AND Silindi = 0 ORDER BY ReferansAdi";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@tur", NormalizeKey(referansTuru));
                        
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                referanslar.Add(new
                                {
                                    Id = dr["Id"].ToString(),
                                    ReferansAdi = dr["ReferansAdi"].ToString()
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Referans verileri başarıyla getirildi: {User}, Tür: {Tur}, Kayıt sayısı: {Count}", 
                    kullaniciAdi, referansTuru, referanslar.Count);
                return Json(new { success = true, data = referanslar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans verileri getirilirken hata oluştu: {User}, Tür: {Tur}", 
                    kullaniciAdi, referansTuru);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Guncelle(int id, string yeniAd)
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Referans güncelleme işlemi başlatıldı: {User}, ID: {Id}, Yeni Ad: {YeniAd}", 
                kullaniciAdi ?? "Anonim", id, yeniAd);
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Referanslar SET ReferansAdi = @yeniAd WHERE Id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@yeniAd", yeniAd);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                _logger.LogInformation("Referans başarıyla güncellendi: {User}, ID: {Id}, Yeni Ad: {YeniAd}", 
                    kullaniciAdi, id, yeniAd);
                return Json(new { success = true, message = "Güncelleme başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans güncelleme hatası: {User}, ID: {Id}, Yeni Ad: {YeniAd}", 
                    kullaniciAdi, id, yeniAd);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Sil(int id)
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Referans silme işlemi başlatıldı: {User}, ID: {Id}", 
                kullaniciAdi ?? "Anonim", id);
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Referanslar SET Silindi = 1 WHERE Id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                _logger.LogInformation("Referans başarıyla silindi: {User}, ID: {Id}", 
                    kullaniciAdi, id);
                return Json(new { success = true, message = "Silme başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Referans silme hatası: {User}, ID: {Id}", 
                    kullaniciAdi, id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetTurkishName(string englishName)
        {
            return englishName switch
            {
                "BasvuranBirim" => "Başvuran Birim",
                "BasvuruYapilanProje" => "Başvuru Yapılan Proje",
                "BasvuruYapilanTur" => "Başvuru Yapılan Tür",
                "KatilimciTuru" => "Katılımcı Türü",
                "BasvuruDonemi" => "Başvuru Dönemi",
                "BasvuruDurumu" => "Başvuru Durumu",
                _ => englishName
            };
        }

        // Referans türü anahtarlarını normalize eder (Türkçe karakterleri ASCII'ye dönüştürür)
        private string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            return key
                .Replace("İ", "I").Replace("I", "I")
                .Replace("ı", "i")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ü", "u").Replace("Ü", "U")
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ç", "c").Replace("Ç", "C");
        }
    }
}
