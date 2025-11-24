using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    public class PlantasController : Controller
    {
        private readonly IRepositorioPlantas repositorioPlantas;
        private readonly IRepositorioMiJardin repositorioMiJardin;

        public PlantasController(
            IRepositorioPlantas repositorioPlantas,
            IRepositorioMiJardin repositorioMiJardin)
        {
            this.repositorioPlantas = repositorioPlantas;
            this.repositorioMiJardin = repositorioMiJardin;
        }

        private int? ObtenerUsuarioId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.Parse(userIdClaim ?? "0");
            }
            return null;
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

            var usuarioId = ObtenerUsuarioId();
            if (usuarioId.HasValue)
            {
                planta.EstaEnMiJardin = await repositorioMiJardin.PlantaEstaEnJardin(usuarioId.Value, id);
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