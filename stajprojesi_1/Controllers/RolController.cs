using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using stajprojesi_1.Models; // Rol modelinin namespace'i burası olmalı

namespace stajprojesi_1.Controllers
{
    public class RolController : BaseController
    {
        private readonly string connectionString;

        public RolController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            List<Role> roller = new List<Role>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name FROM Roles";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roller.Add(new Role
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return View(roller);
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
