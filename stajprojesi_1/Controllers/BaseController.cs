
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using System.Data;
using stajprojesi_1.Models;
using Serilog;

namespace stajprojesi_1.Controllers
{
    public class BaseController : Controller
    {
        private string connectionString = "Server=.;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly Serilog.ILogger _logger;

        public BaseController()
        {
            _logger = Log.ForContext<BaseController>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName;
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi") ?? "Anonim";
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var path = HttpContext.Request?.Path.Value ?? "Unknown";
            var method = HttpContext.Request?.Method ?? "Unknown";

            // Base controller logu
            _logger.Information("🔍 BASE CONTROLLER: {Controller}.{Action} | 👤 Kullanıcı: {User} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP} | 🔍 User Agent: {UserAgent}",
                controllerName, actionName, kullaniciAdi, method, path, ipAddress, userAgent);

            System.Diagnostics.Debug.WriteLine($"=== BASE CONTROLLER OnActionExecuting ===");
            System.Diagnostics.Debug.WriteLine($"Controller: {controllerName}");
            System.Diagnostics.Debug.WriteLine($"Action: {actionName}");

            var rolIdFromSession = HttpContext.Session.GetInt32("RolId");
            
            System.Diagnostics.Debug.WriteLine($"BASE SESSION - KullaniciAdi: {kullaniciAdi}, RolId: {rolIdFromSession}");
            
            if (rolIdFromSession == null)
            {
                _logger.Warning("⚠️ SESSION YOK: {Controller}.{Action} | 👤 Kullanıcı: {User} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP}",
                    controllerName, actionName, kullaniciAdi, method, path, ipAddress);
                
                System.Diagnostics.Debug.WriteLine("BASE: Session yoksa sadece menüleri boş bırak, action'ı durdurma");
                // Session yoksa sadece menüleri boş bırak, action'ı durdurma
                ViewBag.Menus = new List<Menu>();
                ViewBag.SubMenus = null;
                return;
            }

            int rolId = rolIdFromSession.Value;
            _logger.Information("✅ SESSION VAR: {Controller}.{Action} | 👤 Kullanıcı: {User} | 🆔 Rol ID: {RolId} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP}",
                controllerName, actionName, kullaniciAdi, rolId, method, path, ipAddress);

            try
            {
                var menus = GetUserMenus(rolId);
                ViewBag.Menus = menus;
                ViewBag.SubMenus = null;
                
                _logger.Information("📋 MENÜ YÜKLENDİ: {Controller}.{Action} | 👤 Kullanıcı: {User} | 🆔 Rol ID: {RolId} | 📊 Menü Sayısı: {MenuCount}",
                    controllerName, actionName, kullaniciAdi, rolId, menus.Count);
            }
            catch (Exception ex)
            {
                // Menü yükleme hatası olursa sadece boş bırak
                _logger.Error(ex, "❌ MENÜ YÜKLEME HATASI: {Controller}.{Action} | 👤 Kullanıcı: {User} | 🆔 Rol ID: {RolId} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP}",
                    controllerName, actionName, kullaniciAdi, rolId, method, path, ipAddress);
                
                System.Diagnostics.Debug.WriteLine($"Menu loading error: {ex.Message}");
                ViewBag.Menus = new List<Menu>();
                ViewBag.SubMenus = null;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName;
            var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi") ?? "Anonim";
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var path = HttpContext.Request?.Path.Value ?? "Unknown";
            var method = HttpContext.Request?.Method ?? "Unknown";
            var statusCode = context.HttpContext.Response?.StatusCode ?? 0;

            // Action tamamlanma logu
            if (context.Exception != null)
            {
                _logger.Error(context.Exception, "💥 ACTION HATASI: {Controller}.{Action} | 👤 Kullanıcı: {User} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP} | 📊 HTTP Status: {StatusCode}",
                    controllerName, actionName, kullaniciAdi, method, path, ipAddress, statusCode);
            }
            else
            {
                _logger.Information("✅ ACTION TAMAMLANDI: {Controller}.{Action} | 👤 Kullanıcı: {User} | 📍 Yol: {Method} {Path} | 🌍 IP: {IP} | 📊 HTTP Status: {StatusCode}",
                    controllerName, actionName, kullaniciAdi, method, path, ipAddress, statusCode);
            }
        }

        private List<Menu> GetUserMenus(int rolId)
        {
            List<Menu> menus = new List<Menu>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT m.Id, m.Name, m.Url, m.Icon, m.ParentId
                        FROM Menus m
                        INNER JOIN RoleMenus rm ON m.Id = rm.MenuId
                        WHERE rm.RoleId = @roleId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@roleId", rolId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                menus.Add(new Menu
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    Url = reader["Url"].ToString(),
                                    Icon = reader["Icon"].ToString(),
                                    ParentId = reader["ParentId"] != DBNull.Value ? Convert.ToInt32(reader["ParentId"]) : (int?)null
                                });
                            }
                        }
                    }
                }

                _logger.Debug("🔍 MENÜ VERİTABANINDAN ALINDI: 🆔 Rol ID: {RolId} | 📊 Menü Sayısı: {MenuCount}", rolId, menus.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ MENÜ VERİTABANI HATASI: 🆔 Rol ID: {RolId}", rolId);
                throw;
            }

            return menus;
        }
    }
}
