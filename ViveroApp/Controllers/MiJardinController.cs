using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    [Authorize]
    public class MiJardinController : Controller
    {
        private readonly IRepositorioMiJardin repositorioMiJardin;
        private readonly IRepositorioPlantas repositorioPlantas;

        public MiJardinController(
            IRepositorioMiJardin repositorioMiJardin,
            IRepositorioPlantas repositorioPlantas)
        {
            this.repositorioMiJardin = repositorioMiJardin;
            this.repositorioPlantas = repositorioPlantas;
        }

        private int ObtenerUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioId();
            var plantas = await repositorioMiJardin.ObtenerPlantasUsuario(usuarioId);
            return View(plantas);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(int plantaId, string? ubicacion, string? apodo)
        {
            var usuarioId = ObtenerUsuarioId();

            try
            {
                var miJardinId = await repositorioMiJardin.AgregarPlanta(usuarioId, plantaId, ubicacion, apodo);
                TempData["Mensaje"] = "Planta agregada exitosamente a tu jardín";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar la planta: " + ex.Message;
                return RedirectToAction("DetallePlanta", "Plantas", new { id = plantaId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = ObtenerUsuarioId();

            var resultado = await repositorioMiJardin.EliminarPlanta(id, usuarioId);

            if (resultado)
            {
                TempData["Mensaje"] = "Planta eliminada de tu jardín";
            }
            else
            {
                TempData["Error"] = "No se pudo eliminar la planta";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(int id, string? ubicacion, string? apodo)
        {
            var usuarioId = ObtenerUsuarioId();

            var resultado = await repositorioMiJardin.ActualizarPlanta(id, usuarioId, ubicacion, apodo);

            if (resultado)
            {
                TempData["Mensaje"] = "Información actualizada correctamente";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar la información";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarRiego(int id)
        {
            var usuarioId = ObtenerUsuarioId();

            var resultado = await repositorioMiJardin.RegistrarRiego(id, usuarioId);

            if (resultado)
            {
                TempData["Mensaje"] = "Riego registrado exitosamente";
            }
            else
            {
                TempData["Error"] = "No se pudo registrar el riego";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            try
            {
                var usuarioId = ObtenerUsuarioId();

                if (ids == null || ids.Length == 0)
                {
                    return BadRequest("No se recibieron IDs");
                }

                var idsString = string.Join(",", ids);

                await repositorioMiJardin.Ordenar(idsString, usuarioId);

                return Ok(new { success = true, message = "Orden actualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
