using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using stajprojesi_1.Models;

namespace stajprojesi_1.Controllers
{
    public class MenuController : BaseController
    {
        private readonly string connectionString;

        public MenuController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            List<Menu> menuler = new List<Menu>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name, Url FROM Menus";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuler.Add(new Menu
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Url = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return View(menuler);
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
