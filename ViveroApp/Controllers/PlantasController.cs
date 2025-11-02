using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ViveroApp.Servicios;
using ViveroApp.Models;
using Dapper;

namespace ViveroApp.Controllers
{
    public class PlantasController : Controller
    {
        private readonly IRepositorioPlantas repositorioPlantas;
        private string _connectionString;

        public PlantasController(IRepositorioPlantas repositorioPlantas)
        {
            this.repositorioPlantas = repositorioPlantas;
        }

        public async Task<IActionResult> Index()
        {
            var plantas = await repositorioPlantas.ObtenerTodasLasPlantas(); 
            return View(plantas);
        }

        public async Task<IActionResult> DetallePlanta(int id)
        {
            var planta = await repositorioPlantas.ObtenerDetalle(id);

            if (planta == null)
            {
                return NotFound();
            }

            return View(planta);
        }

        public async Task<IActionResult> PlantasPopulares()
        {
            var plantas = await repositorioPlantas.ObtenerPlantasPopulares();
            return View(plantas);
        }



    }
}
