using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using stajprojesi_1.Controllers;
using stajprojesi_1.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

public class YetkiController : BaseController
{
    private readonly string connectionString = "Server=localhost;Database=stajprojesi_1;Trusted_Connection=True;TrustServerCertificate=True";
    private readonly ILogger<YetkiController> _logger;

    public YetkiController(ILogger<YetkiController> logger)
    {
        _logger = logger;
    }

    public IActionResult Yetki()
    {
        var roller = new List<Role>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "SELECT Id, Name FROM Roles";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    roller.Add(new Role
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString()
                    });
                }
            }
        }

        return View(roller);
    }

    public IActionResult Duzenle(int id)
    {
        var model = new RoleMenuEditViewModel
        {
            RoleId = id,
            AllMenus = new List<Menu>(),
            SelectedMenuIds = new List<int>()
        };

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Tüm menüleri al
            string menuQuery = "SELECT Id, Name FROM Menus";
            using (SqlCommand cmd = new SqlCommand(menuQuery, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.AllMenus.Add(new Menu
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString()
                    });
                }
            }

            // Rolün eriştiği menü ID’lerini al
            string selectedQuery = "SELECT MenuId FROM RoleMenus WHERE RoleId = @RoleId";
            using (SqlCommand cmd = new SqlCommand(selectedQuery, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.SelectedMenuIds.Add(Convert.ToInt32(reader["MenuId"]));
                    }
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult Duzenle(RoleMenuEditViewModel model)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Önce eski eşleşmeleri sil
            string deleteQuery = "DELETE FROM RoleMenus WHERE RoleId = @RoleId";
            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                cmd.ExecuteNonQuery();
            }

            // Yeni seçilenleri ekle
            foreach (int menuId in model.SelectedMenuIds)
            {
                string insertQuery = "INSERT INTO RoleMenus (RoleId, MenuId) VALUES (@RoleId, @MenuId)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                    cmd.Parameters.AddWithValue("@MenuId", menuId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        return RedirectToAction("Yetki");
    }
}
