using Microsoft.AspNetCore.Mvc;
using Projestaj.Models;
using stajprojesi_1.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace stajprojesi_1.Controllers
{
    public class KullaniciController : BaseController
    {
        string connectionString = "Server=localhost;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpPost]
        public IActionResult KullaniciEkle(Kullanici kullanici)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Username, Email,Password,RolId ) VALUES (@adi, @mail, @sifre, @rolId)";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@adi", kullanici.Username);
                cmd.Parameters.AddWithValue("@mail", kullanici.Email);
                cmd.Parameters.AddWithValue("@sifre", kullanici.Password);
                cmd.Parameters.AddWithValue("@rolId", kullanici.RolId);
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }

            return RedirectToAction("Kullanici");
        }

        [HttpGet]
        public IActionResult Kullanici()
        {
            List<Kullanici> kullaniciList = new List<Kullanici>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT u.Id, u.Username, u.Email, u.Password, u.RolId, r.Name AS RolName
                         FROM Users u
                         LEFT JOIN Roles r ON u.RolId = r.Id
                         WHERE u.IsDeleted = 0";

                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Kullanici k = new Kullanici()
                    {
                        Id = (int)reader["Id"],
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        RolId = Convert.ToInt32(reader["RolId"]),
                        RolName = reader["RolName"].ToString()
                    };
                    kullaniciList.Add(k);
                }
                connection.Close();
            }

            // Roller tablosunu ViewBag'e ekle
            List<Role> roller = new List<Role>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Name FROM Roles";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Role r = new Role()
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        Name = dr["Name"].ToString()
                    };
                    roller.Add(r);
                }
                conn.Close();
            }

            ViewBag.Roller = roller;

            return View(kullaniciList);
        }


        [HttpPost]
        public IActionResult KullaniciSil(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET IsDeleted = 1 WHERE Id = @id"; // Soft delete yapmak istersen: UPDATE Users SET IsDeleted = 1 WHERE Id = @id
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }

            return RedirectToAction("Kullanici");
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

        [HttpPost]
        public IActionResult RolAta(int userId, int roleId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "UPDATE Users SET RolId = @RoleId WHERE Id = @UserId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            // Güncelleme sonrası Kullanici sayfasına dön
            return RedirectToAction("Kullanici", "Kullanici");
        }

    }
}
