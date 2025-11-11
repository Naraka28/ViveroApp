using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViveroApp.Attributes;
using ViveroApp.Dto;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    [Authorize]
    [AdminAuthorization]
    public class AdminController : Controller
    {
        private readonly IRepositorioPlantas repositorioPlantas;
        private readonly IRepositorioAdmin repositorioAdmin;

        public AdminController(
            IRepositorioPlantas repositorioPlantas,
            IRepositorioAdmin repositorioAdmin)
        {
            this.repositorioPlantas = repositorioPlantas;
            this.repositorioAdmin = repositorioAdmin;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await repositorioAdmin.ObtenerEstadisticas();
            return View(stats);
        }

        public async Task<IActionResult> Plantas()
        {
            var plantas = await repositorioAdmin.ObtenerTodasPlantas();
            return View(plantas);
        }

        public IActionResult CrearPlanta()
        {
            ViewBag.Riegos = repositorioAdmin.ObtenerRiegos().Result;
            ViewBag.Luces = repositorioAdmin.ObtenerLuces().Result;
            ViewBag.Sustratos = repositorioAdmin.ObtenerSustratos().Result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearPlanta(CrearPlantaDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Riegos = await repositorioAdmin.ObtenerRiegos();
                ViewBag.Luces = await repositorioAdmin.ObtenerLuces();
                ViewBag.Sustratos = await repositorioAdmin.ObtenerSustratos();
                return View(dto);
            }

            await repositorioAdmin.CrearPlanta(dto);
            TempData["Mensaje"] = "Planta creada exitosamente";
            return RedirectToAction(nameof(Plantas));
        }

        public async Task<IActionResult> EditarPlanta(int id)
        {
            var planta = await repositorioAdmin.ObtenerPlantaPorId(id);
            if (planta == null) return NotFound();

            ViewBag.Riegos = await repositorioAdmin.ObtenerRiegos();
            ViewBag.Luces = await repositorioAdmin.ObtenerLuces();
            ViewBag.Sustratos = await repositorioAdmin.ObtenerSustratos();
            return View(planta);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPlanta(int id, EditarPlantaDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Riegos = await repositorioAdmin.ObtenerRiegos();
                ViewBag.Luces = await repositorioAdmin.ObtenerLuces();
                ViewBag.Sustratos = await repositorioAdmin.ObtenerSustratos();
                return View(dto);
            }

            await repositorioAdmin.ActualizarPlanta(id, dto);
            TempData["Mensaje"] = "Planta actualizada exitosamente";
            return RedirectToAction(nameof(Plantas));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPlanta(int id)
        {
            await repositorioAdmin.EliminarPlanta(id);
            TempData["Mensaje"] = "Planta eliminada exitosamente";
            return RedirectToAction(nameof(Plantas));
        }
        public async Task<IActionResult> Categorias()
        {
            var categorias = await repositorioAdmin.ObtenerCategorias();
            return View(categorias);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria(CategoriaDto dto)
        {
            await repositorioAdmin.CrearCategoria(dto);
            TempData["Mensaje"] = "Categoría creada exitosamente";
            return RedirectToAction(nameof(Categorias));
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCategoria(int id, CategoriaDto dto)
        {
            await repositorioAdmin.ActualizarCategoria(id, dto);
            TempData["Mensaje"] = "Categoría actualizada exitosamente";
            return RedirectToAction(nameof(Categorias));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            await repositorioAdmin.EliminarCategoria(id);
            TempData["Mensaje"] = "Categoría eliminada exitosamente";
            return RedirectToAction(nameof(Categorias));
        }
        public async Task<IActionResult> Riegos()
        {
            var riegos = await repositorioAdmin.ObtenerRiegos();
            return View(riegos);
        }

        public async Task<IActionResult> Luces()
        {
            var luces = await repositorioAdmin.ObtenerLuces();
            return View(luces);
        }

        public async Task<IActionResult> Sustratos()
        {
            var sustratos = await repositorioAdmin.ObtenerSustratos();
            return View(sustratos);
        }

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await repositorioAdmin.ObtenerUsuarios();
            return View(usuarios);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRolUsuario(int id, string nuevoRol)
        {
            await repositorioAdmin.CambiarRolUsuario(id, nuevoRol);
            TempData["Mensaje"] = "Rol actualizado exitosamente";
            return RedirectToAction(nameof(Usuarios));
        }
    }
}