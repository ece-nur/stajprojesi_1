using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using stajprojesi_1.Models;
using System.IO;


namespace stajprojesi_1.Controllers
{
    public class BasvuruController : BaseController
    {
        // MSSQL bağlantı dizesi (kendi sunucu bilgilerinizi yazın)
        private string connectionString = "Server=.;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly ILogger<BasvuruController> _logger;

        public BasvuruController(ILogger<BasvuruController> logger)
        {
            _logger = logger;
        }




        public IActionResult Index()
        {
            // Detaylı session debug
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var rolId = HttpContext.Session.GetInt32("RolId");
            
            _logger.LogInformation("Başvuru sayfası erişim denemesi: {User}, RolId: {RolId}", 
                kullaniciAdi ?? "Anonim", rolId);

            // Session kontrolü - hem KullaniciAdi hem RolId olmalı
            if (string.IsNullOrEmpty(kullaniciAdi) || rolId == null)
            {
                _logger.LogWarning("Yetkisiz başvuru sayfası erişimi, login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("VİEWBAG VERİLERİ YÜKLENİYOR...");
                
                ViewBag.BasvuranBirimListesi = ReferansGetir("BasvuranBirim");
                ViewBag.BasvuruYapilanProjeListesi = ReferansGetir("BasvuruYapilanProje");
                ViewBag.BasvuruTuruListesi = ReferansGetir("BasvuruYapılanTur");
                ViewBag.KatilimciTuruListesi = ReferansGetir("KatılımcıTuru");
                ViewBag.BasvuruDonemiListesi = ReferansGetir("BasvuruDonemi");
                ViewBag.BasvuruDurumuListesi = ReferansGetir("BasvuruDurumu");

                // Debug için
                System.Diagnostics.Debug.WriteLine($"BasvuranBirim count: {((List<SelectListItem>)ViewBag.BasvuranBirimListesi).Count}");
                System.Diagnostics.Debug.WriteLine($"BasvuruTuru count: {((List<SelectListItem>)ViewBag.BasvuruTuruListesi).Count}");
                System.Diagnostics.Debug.WriteLine($"KatilimciTuru count: {((List<SelectListItem>)ViewBag.KatilimciTuruListesi).Count}");
                System.Diagnostics.Debug.WriteLine($"BasvuruDonemi count: {((List<SelectListItem>)ViewBag.BasvuruDonemiListesi).Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ViewBag error: {ex.Message}");
                
                // Fallback veriler
                ViewBag.BasvuranBirimListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Bilgi İşlem", Text = "Bilgi İşlem" },
                    new SelectListItem { Value = "İnsan Kaynakları", Text = "İnsan Kaynakları" }
                };
                
                ViewBag.BasvuruYapilanProjeListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Erasmus", Text = "Erasmus" },
                    new SelectListItem { Value = "Merkezi", Text = "Merkezi" }
                };
                
                ViewBag.BasvuruTuruListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Gençlik", Text = "Gençlik" },
                    new SelectListItem { Value = "Yetişkin", Text = "Yetişkin" }
                };
                
                ViewBag.KatilimciTuruListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Koordinatör", Text = "Koordinatör" },
                    new SelectListItem { Value = "Ortak", Text = "Ortak" }
                };
                
                ViewBag.BasvuruDonemiListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "R1", Text = "R1" }
                };
                
                ViewBag.BasvuruDurumuListesi = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Kabul", Text = "Kabul" },
                    new SelectListItem { Value = "Red", Text = "Red" }
                };
            }

            System.Diagnostics.Debug.WriteLine("VİEW DÖNÜLüYOR");
            return View();
        }

        

        // Test metodu  
        public IActionResult TestComboboxData()
        {
            try
            {
                var result = new
                {
                    BasvuranBirim = ReferansGetir("BasvuranBirim"),
                    BasvuruYapilanProje = ReferansGetir("BasvuruYapilanProje"),
                    BasvuruTuru = ReferansGetir("BasvuruYapılanTur"),
                    KatilimciTuru = ReferansGetir("KatilimciTuru"),
                    BasvuruDonemi = ReferansGetir("BasvuruDonemi"),
                    BasvuruDurumu = ReferansGetir("BasvuruDurumu")
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Session debug test metodu
        public IActionResult SessionTest()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            var rolId = HttpContext.Session.GetInt32("RolId");
            
            var result = new
            {
                KullaniciAdi = kullaniciAdi,
                RolId = rolId,
                SessionId = HttpContext.Session.Id,
                IsSessionActive = !string.IsNullOrEmpty(kullaniciAdi),
                Timestamp = DateTime.Now.ToString()
            };
            
            return Json(result);
        }

        // ViewBag debug test metodu
        public IActionResult ViewBagTest()
        {
            // Session kontrolü atla, sadece ViewBag test et
            try
            {
                ViewBag.BasvuranBirimListesi = ReferansGetir("BasvuranBirim");
                ViewBag.BasvuruYapilanProjeListesi = ReferansGetir("BasvuruYapilanProje");
                ViewBag.BasvuruTuruListesi = ReferansGetir("BasvuruYapılanTur");
                ViewBag.KatilimciTuruListesi = ReferansGetir("KatılımcıTuru");
                ViewBag.BasvuruDonemiListesi = ReferansGetir("BasvuruDonemi");
                ViewBag.BasvuruDurumuListesi = ReferansGetir("BasvuruDurumu");

                var result = new
                {
                    BasvuranBirimCount = ((List<SelectListItem>)ViewBag.BasvuranBirimListesi).Count,
                    BasvuruYapilanProjeCount = ((List<SelectListItem>)ViewBag.BasvuruYapilanProjeListesi).Count,
                    BasvuruTuruCount = ((List<SelectListItem>)ViewBag.BasvuruTuruListesi).Count,
                    KatilimciTuruCount = ((List<SelectListItem>)ViewBag.KatilimciTuruListesi).Count,
                    BasvuruDonemiCount = ((List<SelectListItem>)ViewBag.BasvuruDonemiListesi).Count,
                    BasvuruDurumuCount = ((List<SelectListItem>)ViewBag.BasvuruDurumuListesi).Count,
                    
                    BasvuruTuruItems = ((List<SelectListItem>)ViewBag.BasvuruTuruListesi).Select(x => x.Text).ToList(),
                    KatilimciTuruItems = ((List<SelectListItem>)ViewBag.KatilimciTuruListesi).Select(x => x.Text).ToList()
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        public IActionResult BasvuruIslemleri()
        {
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
            _logger.LogInformation("Başvuru işlemleri sayfası erişimi: {User}", kullaniciAdi ?? "Anonim");

            // Session kontrolü - hem KullaniciAdi hem RolId olmalı
            if (string.IsNullOrEmpty(kullaniciAdi) || 
                HttpContext.Session.GetInt32("RolId") == null)
            {
                _logger.LogWarning("Yetkisiz başvuru işlemleri sayfası erişimi, login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BasvuranBirimListesi = ReferansGetir("BasvuranBirim");
            ViewBag.BasvuruYapilanProjeListesi = ReferansGetir("BasvuruYapilanProje");
            ViewBag.BasvuruTuruListesi = ReferansGetir("BasvuruYapılanTur");
            ViewBag.KatilimciTuruListesi = ReferansGetir("KatılımcıTuru");
            ViewBag.BasvuruDonemiListesi = ReferansGetir("BasvuruDonemi");
            ViewBag.BasvuruDurumuListesi = ReferansGetir("BasvuruDurumu");

            List<BasvuruModel> basvurular = new List<BasvuruModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT Id, Application_unit, ProductName, Application_Product, Application_Type, 
                              Participant_Type, Application_Perio, Application_Date, 
                              Application_Status, Status_Date, [Grant] 
                       FROM dbo.Application
                       WHERE [Delete] = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        basvurular.Add(new BasvuruModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Basvuran_birim = reader["Application_unit"].ToString(),
                            Proje_adi = reader["ProductName"].ToString(),
                            Basvuru_yapilan_proje = reader["Application_Product"].ToString(),
                            Basvuru_yapilan_tur = reader["Application_Type"].ToString(),
                            Katılımcı_turu = reader["Participant_Type"].ToString(),
                            Basvuru_donemi = reader["Application_Perio"].ToString(),
                            Basvuru_tarihi = Convert.ToDateTime(reader["Application_Date"]),
                            Basvuru_durumu = reader["Application_Status"].ToString(),
                            Durum_tarihi = Convert.ToDateTime(reader["Status_Date"]),
                            Hibe_tutari = Convert.ToInt32(reader["Grant"])
                        });
                    }
                }
            }

            return View(basvurular);
        }
        [HttpPost]
        public IActionResult BasvuruListesiFiltre(BasvuruModel filtre)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            List<BasvuruModel> sonuc = new List<BasvuruModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Dinamik SQL oluşturuluyor
                string sql = @"SELECT * FROM dbo.Application WHERE [Delete] = 0";

                if (!string.IsNullOrEmpty(filtre.Proje_adi))
                    sql += " AND ProductName LIKE @Proje_adi";

                if (!string.IsNullOrEmpty(filtre.Basvuran_birim))
                    sql += " AND Application_unit = @Basvuran_birim";

                if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_proje))
                    sql += " AND Application_Product = @Basvuru_yapilan_proje";

                if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_tur))
                    sql += " AND Application_Type = @Basvuru_yapilan_tur";

                if (!string.IsNullOrEmpty(filtre.Katılımcı_turu))
                    sql += " AND Participant_Type = @Katılımcı_turu";

                if (!string.IsNullOrEmpty(filtre.Basvuru_donemi))
                    sql += " AND Application_Perio = @Basvuru_donemi";

                if (filtre.Basvuru_tarihi != DateTime.MinValue)
                    sql += " AND Application_Date = @Basvuru_tarihi";

                if (filtre.Durum_tarihi != DateTime.MinValue)
                    sql += " AND Status_Date = @Durum_tarihi";

                if (!string.IsNullOrEmpty(filtre.Basvuru_durumu))
                    sql += " AND Application_Status = @Basvuru_durumu";

                if (filtre.Hibe_tutari > 0)
                    sql += " AND [Grant] = @Hibe_tutari";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(filtre.Proje_adi))
                        cmd.Parameters.AddWithValue("@Proje_adi", "%" + filtre.Proje_adi + "%");

                    if (!string.IsNullOrEmpty(filtre.Basvuran_birim))
                        cmd.Parameters.AddWithValue("@Basvuran_birim", filtre.Basvuran_birim);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_proje))
                        cmd.Parameters.AddWithValue("@Basvuru_yapilan_proje", filtre.Basvuru_yapilan_proje);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_tur))
                        cmd.Parameters.AddWithValue("@Basvuru_yapilan_tur", filtre.Basvuru_yapilan_tur);

                    if (!string.IsNullOrEmpty(filtre.Katılımcı_turu))
                        cmd.Parameters.AddWithValue("@Katılımcı_turu", filtre.Katılımcı_turu);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_donemi))
                        cmd.Parameters.AddWithValue("@Basvuru_donemi", filtre.Basvuru_donemi);

                    if (filtre.Basvuru_tarihi != DateTime.MinValue)
                        cmd.Parameters.AddWithValue("@Basvuru_tarihi", filtre.Basvuru_tarihi);

                    if (filtre.Durum_tarihi != DateTime.MinValue)
                        cmd.Parameters.AddWithValue("@Durum_tarihi", filtre.Durum_tarihi);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_durumu))
                        cmd.Parameters.AddWithValue("@Basvuru_durumu", filtre.Basvuru_durumu);

                    if (filtre.Hibe_tutari > 0)
                        cmd.Parameters.AddWithValue("@Hibe_tutari", filtre.Hibe_tutari);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sonuc.Add(new BasvuruModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Basvuran_birim = reader["Application_unit"].ToString(),
                                Proje_adi = reader["ProductName"].ToString(),
                                Basvuru_yapilan_proje = reader["Application_Product"].ToString(),
                                Basvuru_yapilan_tur = reader["Application_Type"].ToString(),
                                Katılımcı_turu = reader["Participant_Type"].ToString(),
                                Basvuru_donemi = reader["Application_Perio"].ToString(),
                                Basvuru_tarihi = Convert.ToDateTime(reader["Application_Date"]),
                                Basvuru_durumu = reader["Application_Status"].ToString(),
                                Durum_tarihi = Convert.ToDateTime(reader["Status_Date"]),
                                Hibe_tutari = Convert.ToInt32(reader["Grant"])
                            });
                        }
                    }
                }
            }

            return Json(sonuc);
        }

        [HttpPost]
        public IActionResult ExcelFiltreAktar([FromForm] BasvuruModel filtre)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            List<BasvuruModel> sonuc = new List<BasvuruModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"SELECT * FROM dbo.Application WHERE [Delete] = 0";

                if (!string.IsNullOrEmpty(filtre.Proje_adi))
                    sql += " AND ProductName LIKE @Proje_adi";

                if (!string.IsNullOrEmpty(filtre.Basvuran_birim))
                    sql += " AND Application_unit = @Basvuran_birim";

                if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_proje))
                    sql += " AND Application_Product = @Basvuru_yapilan_proje";

                if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_tur))
                    sql += " AND Application_Type = @Basvuru_yapilan_tur";

                if (!string.IsNullOrEmpty(filtre.Katılımcı_turu))
                    sql += " AND Participant_Type = @Katılımcı_turu";

                if (!string.IsNullOrEmpty(filtre.Basvuru_donemi))
                    sql += " AND Application_Perio = @Basvuru_donemi";

                if (filtre.Basvuru_tarihi != DateTime.MinValue)
                    sql += " AND Application_Date = @Basvuru_tarihi";

                if (filtre.Durum_tarihi != DateTime.MinValue)
                    sql += " AND Status_Date = @Durum_tarihi";

                if (!string.IsNullOrEmpty(filtre.Basvuru_durumu))
                    sql += " AND Application_Status = @Basvuru_durumu";

                if (filtre.Hibe_tutari > 0)
                    sql += " AND [Grant] = @Hibe_tutari";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(filtre.Proje_adi))
                        cmd.Parameters.AddWithValue("@Proje_adi", "%" + filtre.Proje_adi + "%");

                    if (!string.IsNullOrEmpty(filtre.Basvuran_birim))
                        cmd.Parameters.AddWithValue("@Basvuran_birim", filtre.Basvuran_birim);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_proje))
                        cmd.Parameters.AddWithValue("@Basvuru_yapilan_proje", filtre.Basvuru_yapilan_proje);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_yapilan_tur))
                        cmd.Parameters.AddWithValue("@Basvuru_yapilan_tur", filtre.Basvuru_yapilan_tur);

                    if (!string.IsNullOrEmpty(filtre.Katılımcı_turu))
                        cmd.Parameters.AddWithValue("@Katılımcı_turu", filtre.Katılımcı_turu);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_donemi))
                        cmd.Parameters.AddWithValue("@Basvuru_donemi", filtre.Basvuru_donemi);

                    if (filtre.Basvuru_tarihi != DateTime.MinValue)
                        cmd.Parameters.AddWithValue("@Basvuru_tarihi", filtre.Basvuru_tarihi);

                    if (filtre.Durum_tarihi != DateTime.MinValue)
                        cmd.Parameters.AddWithValue("@Durum_tarihi", filtre.Durum_tarihi);

                    if (!string.IsNullOrEmpty(filtre.Basvuru_durumu))
                        cmd.Parameters.AddWithValue("@Basvuru_durumu", filtre.Basvuru_durumu);

                    if (filtre.Hibe_tutari > 0)
                        cmd.Parameters.AddWithValue("@Hibe_tutari", filtre.Hibe_tutari);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sonuc.Add(new BasvuruModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Basvuran_birim = reader["Application_unit"].ToString(),
                                Proje_adi = reader["ProductName"].ToString(),
                                Basvuru_yapilan_proje = reader["Application_Product"].ToString(),
                                Basvuru_yapilan_tur = reader["Application_Type"].ToString(),
                                Katılımcı_turu = reader["Participant_Type"].ToString(),
                                Basvuru_donemi = reader["Application_Perio"].ToString(),
                                Basvuru_tarihi = Convert.ToDateTime(reader["Application_Date"]),
                                Basvuru_durumu = reader["Application_Status"].ToString(),
                                Durum_tarihi = Convert.ToDateTime(reader["Status_Date"]),
                                Hibe_tutari = Convert.ToInt32(reader["Grant"])
                            });
                        }
                    }
                }
            }

            // Excel oluştur
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Başvurular");

                ws.Cell(1, 1).Value = "Başvuran Birim";
                ws.Cell(1, 2).Value = "Proje Adı";
                ws.Cell(1, 3).Value = "Başvuru Yapılan Proje";
                ws.Cell(1, 4).Value = "Başvuru Yapılan Tür";
                ws.Cell(1, 5).Value = "Katılımcı Türü";
                ws.Cell(1, 6).Value = "Başvuru Dönemi";
                ws.Cell(1, 7).Value = "Başvuru Tarihi";
                ws.Cell(1, 8).Value = "Başvuru Durumu";
                ws.Cell(1, 9).Value = "Durum Tarihi";
                ws.Cell(1, 10).Value = "Hibe Tutarı";

                int row = 2;
                foreach (var item in sonuc)
                {
                    ws.Cell(row, 1).Value = item.Basvuran_birim;
                    ws.Cell(row, 2).Value = item.Proje_adi;
                    ws.Cell(row, 3).Value = item.Basvuru_yapilan_proje;
                    ws.Cell(row, 4).Value = item.Basvuru_yapilan_tur;
                    ws.Cell(row, 5).Value = item.Katılımcı_turu;
                    ws.Cell(row, 6).Value = item.Basvuru_donemi;
                    ws.Cell(row, 7).Value = item.Basvuru_tarihi.ToShortDateString();
                    ws.Cell(row, 8).Value = item.Basvuru_durumu;
                    ws.Cell(row, 9).Value = item.Durum_tarihi.ToShortDateString();
                    ws.Cell(row, 10).Value = item.Hibe_tutari;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Basvurular.xlsx");
                }
            }
        }


        [HttpPost]
        public IActionResult BasvuruKaydet([FromForm] BasvuruModel model)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"INSERT INTO dbo.Application 
    (Application_unit, ProductName, Application_Product, Application_Type, 
     Participant_Type, Application_Perio, Application_Date, 
     Application_Status, Status_Date, [Grant]) 
    VALUES 
    (@Application_unit, @ProductName, @Application_Product, @Application_Type, 
     @Participant_Type, @Application_Perio, @Application_Date, 
     @Application_Status, @Status_Date, @Grant)";


                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Application_unit", model.Basvuran_birim);
                        cmd.Parameters.AddWithValue("@ProductName", model.Proje_adi);
                        cmd.Parameters.AddWithValue("@Application_Product", model.Basvuru_yapilan_proje);
                        cmd.Parameters.AddWithValue("@Application_Type", model.Basvuru_yapilan_tur);
                        cmd.Parameters.AddWithValue("@Participant_Type", model.Katılımcı_turu);
                        cmd.Parameters.AddWithValue("@Application_Perio", model.Basvuru_donemi);
                        cmd.Parameters.AddWithValue("@Application_Date", model.Basvuru_tarihi);
                        cmd.Parameters.AddWithValue("@Application_Status", model.Basvuru_durumu);
                        cmd.Parameters.AddWithValue("@Status_Date", model.Durum_tarihi);
                        cmd.Parameters.AddWithValue("@Grant", model.Hibe_tutari);

                        cmd.ExecuteNonQuery();
                    }
                }

                return Json("Veri başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                return BadRequest("Kaydetme hatası: " + ex.Message);
            }




        }
        public IActionResult GetBasvuru(int id)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            BasvuruModel basvuru = null;

            string connectionString = "Server=.;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Application WHERE Id = @id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    basvuru = new BasvuruModel
                    {
                        Id = (int)reader["Id"],
                        Proje_adi = reader["ProductName"].ToString(),
                        Basvuran_birim = reader["Application_unit"].ToString(),
                        Basvuru_yapilan_proje = reader["Application_Product"].ToString(),
                        Basvuru_yapilan_tur = reader["Application_Type"].ToString(),
                        Katılımcı_turu = reader["Participant_Type"].ToString(),
                        Basvuru_donemi = reader["Application_Perio"].ToString(),
                        Basvuru_tarihi = Convert.ToDateTime(reader["Application_Date"]),
                        Basvuru_durumu = reader["Application_Status"].ToString(),
                        Durum_tarihi = Convert.ToDateTime(reader["Status_Date"]),
                        Hibe_tutari = Convert.ToInt32(reader["Grant"])
                    };
                }

                reader.Close();
            }

            return Json(basvuru);
        }

        [HttpPost]
        public IActionResult UpdateBasvuru(BasvuruModel model)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"UPDATE dbo.Application SET 
            ProductName= @Proje_adi,
            Application_unit = @Basvuran_birim,
            Application_Product = @Basvuru_yapilan_proje,
            Application_Type = @Basvuru_yapilan_tur,
            Participant_Type = @Katılımcı_turu,
            Application_Perio = @Basvuru_donemi,
            Application_Date = @Basvuru_tarihi,
            Application_Status = @Basvuru_durumu,
            Status_Date = @Durum_tarihi,
            [Grant] = @Hibe_tutari
            WHERE Id = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", model.Id);
                command.Parameters.AddWithValue("@Proje_adi", model.Proje_adi ?? "");
                command.Parameters.AddWithValue("@Basvuran_birim", model.Basvuran_birim ?? "");
                command.Parameters.AddWithValue("@Basvuru_yapilan_proje", model.Basvuru_yapilan_proje ?? "");
                command.Parameters.AddWithValue("@Basvuru_yapilan_tur", model.Basvuru_yapilan_tur ?? "");
                command.Parameters.AddWithValue("@Katılımcı_turu", model.Katılımcı_turu ?? "");
                command.Parameters.AddWithValue("@Basvuru_donemi", model.Basvuru_donemi ?? "");
                command.Parameters.AddWithValue("@Basvuru_tarihi", model.Basvuru_tarihi);
                command.Parameters.AddWithValue("@Basvuru_durumu", model.Basvuru_durumu ?? "");
                command.Parameters.AddWithValue("@Durum_tarihi", model.Durum_tarihi);
                command.Parameters.AddWithValue("@Hibe_tutari", model.Hibe_tutari );

                connection.Open();
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult BasvuruSil(int id)
        {
            if (HttpContext.Session.GetString("KullaniciAdi") == null)
                return RedirectToAction("Index", "Login");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE dbo.Application SET [Delete] = 1 WHERE Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Başvuru silindi." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Kayıt bulunamadı." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        private List<SelectListItem> ReferansGetir(string referansTuru)
        {
            List<SelectListItem> liste = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ReferansAdi FROM Referanslar WHERE Silindi = 0 AND ReferansTuru = @tur";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tur", NormalizeKey(referansTuru));

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            liste.Add(new SelectListItem
                            {
                                Value = dr["ReferansAdi"].ToString(),
                                Text = dr["ReferansAdi"].ToString()
                            });
                        }
                    }
                }
            }

            return liste;
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
