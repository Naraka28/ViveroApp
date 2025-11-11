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
        private readonly IServicioArchivos servicioArchivos;

        public AdminController(
            IRepositorioPlantas repositorioPlantas,
            IRepositorioAdmin repositorioAdmin,
            IServicioArchivos servicioArchivos)
        {
            this.repositorioPlantas = repositorioPlantas;
            this.repositorioAdmin = repositorioAdmin;
            this.servicioArchivos = servicioArchivos;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await repositorioAdmin.ObtenerEstadisticas();
            return View(stats);
        }

        public async Task<IActionResult> Plantas()
        {
            var plantas = await repositorioAdmin.ObtenerTodasPlantas();
            return View("Plantas/Index", plantas);
        }

        public async Task<IActionResult> CrearPlanta()
        {
            await CargarDatosPlanta();
            return View("Plantas/Crear");
        }

        [HttpPost]
        public async Task<IActionResult> CrearPlanta(CrearPlantaDto dto)
        {
            if (dto.ImagenFile != null)
            {
                if (!servicioArchivos.ValidarImagen(dto.ImagenFile))
                {
                    ModelState.AddModelError(nameof(dto.ImagenFile), "Archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    dto.ImagenUrl = await servicioArchivos.GuardarImagen(dto.ImagenFile, "");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosPlanta();
                return View("Plantas/Crear", dto);
            }

            await repositorioAdmin.CrearPlanta(dto);
            TempData["Mensaje"] = "Planta creada exitosamente";
            return RedirectToAction(nameof(Plantas));
        }

        public async Task<IActionResult> EditarPlanta(int id)
        {
            var planta = await repositorioAdmin.ObtenerPlantaPorId(id);
            if (planta == null) return NotFound();

            await CargarDatosPlanta();
            return View("Plantas/Editar", planta);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPlanta(int id, EditarPlantaDto dto)
        {
            if (dto.ImagenFile != null)
            {
                if (!servicioArchivos.ValidarImagen(dto.ImagenFile))
                {
                    ModelState.AddModelError(nameof(dto.ImagenFile), "Nuevo archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    var nuevaRuta = await servicioArchivos.GuardarImagen(dto.ImagenFile, "");

                    if (!string.IsNullOrEmpty(dto.ImagenUrl))
                    {
                        await servicioArchivos.EliminarImagen(dto.ImagenUrl);
                    }
                    dto.ImagenUrl = nuevaRuta;
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosPlanta();

                var plantaOriginal = await repositorioAdmin.ObtenerPlantaPorId(id);
                dto.CategoriasIds = plantaOriginal.CategoriasIds;
                if (dto.ImagenFile == null)
                {
                    dto.ImagenUrl = plantaOriginal.ImagenUrl;
                }

                return View("Plantas/Editar", dto);
            }

            await repositorioAdmin.ActualizarPlanta(id, dto);
            TempData["Mensaje"] = "Planta actualizada exitosamente";
            return RedirectToAction(nameof(Plantas));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPlanta(int id)
        {
            var planta = await repositorioAdmin.ObtenerPlantaPorId(id);
            if (planta == null)
            {
                TempData["Error"] = "La planta no fue encontrada.";
                return RedirectToAction(nameof(Plantas));
            }
            await repositorioAdmin.EliminarPlanta(id);

            if (!string.IsNullOrEmpty(planta.ImagenUrl))
            {
                await servicioArchivos.EliminarImagen(planta.ImagenUrl); 
            }
            TempData["Mensaje"] = "Planta eliminada exitosamente"; 
            return RedirectToAction(nameof(Plantas)); 
        }

        public async Task<IActionResult> Categorias()
        {
            var categorias = await repositorioAdmin.ObtenerCategorias();
            return View("Categorias/Index", categorias);
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

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await repositorioAdmin.ObtenerUsuarios();
            return View("Usuarios/Index", usuarios);
        }

        public IActionResult CrearUsuario()
        {
            return View("Usuarios/Crear");
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Usuarios/Crear", dto);
            }

            await repositorioAdmin.CrearUsuario(dto);
            TempData["Mensaje"] = "Usuario creado exitosamente";
            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> EditarUsuario(int id)
        {
            var usuario = await repositorioAdmin.ObtenerUsuarioPorId(id);
            if (usuario == null) return NotFound();

            return View("Usuarios/Editar", usuario);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(int id, EditarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Usuarios/Editar", dto);
            }

            await repositorioAdmin.ActualizarUsuario(id, dto);
            TempData["Mensaje"] = "Usuario actualizado exitosamente";
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            await repositorioAdmin.EliminarUsuario(id);
            TempData["Mensaje"] = "Usuario eliminado exitosamente";
            return RedirectToAction(nameof(Usuarios));
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRolUsuario(int id, string nuevoRol)
        {
            await repositorioAdmin.CambiarRolUsuario(id, nuevoRol);
            TempData["Mensaje"] = "Rol actualizado exitosamente";
            return RedirectToAction(nameof(Usuarios));
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

        private async Task CargarDatosPlanta()
        {
            ViewBag.Riegos = await repositorioAdmin.ObtenerRiegos();
            ViewBag.Luces = await repositorioAdmin.ObtenerLuces();
            ViewBag.Sustratos = await repositorioAdmin.ObtenerSustratos();
            ViewBag.Categorias = await repositorioAdmin.ObtenerCategorias();
        }
    }
}