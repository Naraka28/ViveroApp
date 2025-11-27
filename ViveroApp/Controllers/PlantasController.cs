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
                var resultado = await plantIdService.IdentificarPlanta(
                    request.ImagenBase64,
                    request.Latitude,
                    request.Longitude
                );

                // Mostrar resultados incluso si hay errores parciales
                if (resultado == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se pudo conectar con el servicio de identificación."
                    });
                }

                // Si hay sugerencias, mostrarlas
                var tieneSugerencias = resultado.Result?.Classification?.Suggestions?.Count > 0;

                if (!tieneSugerencias)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se encontraron coincidencias para esta imagen."
                    });
                }

                // SIEMPRE devolver éxito si hay sugerencias, independientemente de IsPlantBinary
                return Json(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en controlador: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"Error al identificar la planta: {ex.Message}"
                });
            }
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

    // Clase DTO fuera del controlador
    public class IdentificarPlantaRequest
    {
        public string ImagenBase64 { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}