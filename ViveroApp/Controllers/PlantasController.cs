using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ViveroApp.Models;

namespace ViveroApp.Controllers
{
    public class PlantasController : Controller
    {
        private readonly string connectionString;

        public PlantasController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IActionResult> Index()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var plantas = await connection.QueryAsync<Plantas>("SELECT * FROM planta");
                return View(plantas);
            }
        }
    }
}