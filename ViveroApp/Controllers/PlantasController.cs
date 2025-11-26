using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    public class PlantasController : Controller
    {
        private readonly IRepositorioPlantas repositorioPlantas;
        private readonly IRepositorioMiJardin repositorioMiJardin;
        private readonly IPlantIdService plantIdService;

        public PlantasController(
            IRepositorioPlantas repositorioPlantas,
            IRepositorioMiJardin repositorioMiJardin,
            IPlantIdService plantIdService)
        {
            this.repositorioPlantas = repositorioPlantas;
            this.repositorioMiJardin = repositorioMiJardin;
            this.plantIdService = plantIdService;
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

        [HttpGet]
        public async Task<IActionResult> BuscarTexto(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { success = false, message = "Término de búsqueda vacío" });
            }

            var resultados = await repositorioPlantas.BuscarPlantas(q);
            return Json(new { success = true, data = resultados });
        }

        [HttpPost]
        public async Task<IActionResult> IdentificarPorImagen([FromBody] IdentificarPlantaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ImagenBase64))
            {
                return Json(new { success = false, message = "Imagen no proporcionada" });
            }

            try
            {
                // Remover el prefijo data:image si existe
                var imagenBase64 = request.ImagenBase64;
                if (imagenBase64.Contains(","))
                {
                    imagenBase64 = imagenBase64.Split(',')[1];
                }

                var resultado = await plantIdService.IdentificarPlanta(
                    imagenBase64,
                    request.Latitude,
                    request.Longitude
                );

                if (resultado == null || !resultado.Result.Is_plant)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se pudo identificar una planta en la imagen"
                    });
                }

                return Json(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al identificar la planta: {ex.Message}"
                });
            }
        }
    }

    public class IdentificarPlantaRequest
    {
        public string ImagenBase64 { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}