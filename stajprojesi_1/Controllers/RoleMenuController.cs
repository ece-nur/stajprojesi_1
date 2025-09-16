using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using stajprojesi_1.Models;

namespace stajprojesi_1.Controllers
{
    public class RoleMenuController : BaseController
    {
        private readonly string connectionString;

        public RoleMenuController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public class RoleMenuViewModel
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public int MenuId { get; set; }
            public string MenuName { get; set; }
        }

        public IActionResult Index()
        {
            List<RoleMenuViewModel> roleMenuler = new List<RoleMenuViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT rm.RoleId, r.Name AS RoleName, rm.MenuId, m.Name AS MenuName
                    FROM RoleMenus rm
                    INNER JOIN Roles r ON rm.RoleId = r.Id
                    INNER JOIN Menus m ON rm.MenuId = m.Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roleMenuler.Add(new RoleMenuViewModel
                            {
                                RoleId = reader.GetInt32(0),
                                RoleName = reader.GetString(1),
                                MenuId = reader.GetInt32(2),
                                MenuName = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return View(roleMenuler);
        }

        public IActionResult SomeAction()
        {
            var rolId = HttpContext.Session.GetInt32("RolId");
            if (rolId == null)
            {
                // Giriş yapılmamış, login sayfasına gönder
                return RedirectToAction("Login", "Account");
            }

            // İzin verilen roller listesi (örnek)
            var izinliRolIdler = new List<int> { 1, 2 }; // 1=Admin, 2=ÜstDüzey kullanıcı

            if (!izinliRolIdler.Contains(rolId.Value))
            {
                // Yetkisiz erişim
                return RedirectToAction("AccessDenied", "Account"); // ya da hata sayfası
            }

            return View();
        }
    }
}