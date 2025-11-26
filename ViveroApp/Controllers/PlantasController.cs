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

        public async Task<IActionResult> Categoria(string nombre)
        {
            var resultado = await repositorioPlantas.ObtenerPlantasPorCategoria(nombre);

            if (resultado == null || !resultado.Plantas.Any())
            {
                return NotFound();
            }

            return View(resultado);
        }

        public async Task<IActionResult> Recomendacion(string tipo)
        {
            var plantas = await repositorioPlantas.ObtenerPlantasPorRecomendacion(tipo);

            var titulo = tipo switch
            {
                "Principiantes" => "Plantas para Principiantes",
                "PocaLuz" => "Plantas de Poca Luz",
                "BajoRiego" => "Plantas de Bajo Riego",
                _ => "Recomendaciones"
            };

            var descripcion = tipo switch
            {
                "Principiantes" => "Plantas resistentes y de bajo mantenimiento perfectas para empezar",
                "PocaLuz" => "Ideales para espacios interiores con luz indirecta",
                "BajoRiego" => "Perfectas si viajas seguido o prefieres regar poco",
                _ => "Plantas recomendadas para ti"
            };

            ViewBag.Titulo = titulo;
            ViewBag.Descripcion = descripcion;

            return View("Recomendacion", plantas);
        }
    }
}