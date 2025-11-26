using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViveroApp.Attributes;
using ViveroApp.Dto;
using ViveroApp.Models;
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

        public async Task<IActionResult> Plantas([FromQuery] FiltroPlantasDto? filtro)
        {
            if (filtro == null )
            {
                var plantas = await repositorioAdmin.ObtenerTodasPlantas();
                return View("Plantas/Index", plantas);
            }

            var resultado = await repositorioAdmin.ObtenerTodasPlantasPaginadas(filtro);
            return View("Plantas/Index", resultado);
        }

        public async Task<IActionResult> Categorias([FromQuery] FiltroCategoriasDto? filtro)
        {
            if (filtro == null)
            {
                var categorias = await repositorioAdmin.ObtenerCategorias();
                return View("Categorias/Index", categorias);
            }

            var resultado = await repositorioAdmin.ObtenerCategoriasPaginadas(filtro);
            return View("Categorias/Index", resultado);
        }

        public async Task<IActionResult> Usuarios([FromQuery] FiltroUsuariosDto? filtro)
        {
            if (filtro == null)
            {
                var usuarios = await repositorioAdmin.ObtenerUsuarios();
                return View("Usuarios/Index", usuarios);
            }

            var resultado = await repositorioAdmin.ObtenerUsuariosPaginados(filtro);
            return View("Usuarios/Index", resultado);
        }




        public async Task<IActionResult> Index()
        {
            var stats = await repositorioAdmin.ObtenerEstadisticas();
            return View(stats);
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
                    ModelState.AddModelError(nameof(dto.ImagenFile),
                        "Archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    dto.ImagenUrl = await servicioArchivos.GuardarImagen(dto.ImagenFile, "plantas");
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
                    ModelState.AddModelError(nameof(dto.ImagenFile),
                        "Nuevo archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    var nuevaRuta = await servicioArchivos.GuardarImagen(dto.ImagenFile, "plantas");

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

        public IActionResult CrearCategoria()
        {
            return View("Categorias/Crear");
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria(CategoriaDto dto)
        {
            if (dto.ImagenFile != null)
            {
                if (!servicioArchivos.ValidarImagen(dto.ImagenFile))
                {
                    ModelState.AddModelError(nameof(dto.ImagenFile),
                        "Archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    dto.Icono = await servicioArchivos.GuardarImagen(dto.ImagenFile, "categorias");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Categorias/Crear", dto);
            }

            await repositorioAdmin.CrearCategoria(dto);
            TempData["Mensaje"] = "Categoría creada exitosamente";
            return RedirectToAction(nameof(Categorias));
        }

        public async Task<IActionResult> EditarCategoria(int id)
        {
            var categoria = await repositorioAdmin.ObtenerCategoriaPorId(id);
            if (categoria == null) return NotFound();

            return View("Categorias/Editar", categoria);
        }

        [HttpPost]
        public async Task<IActionResult> EditarCategoria(int id, CategoriaDto dto)
        {
            if (dto.ImagenFile != null)
            {
                if (!servicioArchivos.ValidarImagen(dto.ImagenFile))
                {
                    ModelState.AddModelError(nameof(dto.ImagenFile),
                        "Nuevo archivo de imagen inválido (debe ser .jpg, .png, .webp, etc. y pesar menos de 5MB).");
                }
                else
                {
                    var nuevaRuta = await servicioArchivos.GuardarImagen(dto.ImagenFile, "categorias");

                    if (!string.IsNullOrEmpty(dto.Icono))
                    {
                        await servicioArchivos.EliminarImagen(dto.Icono);
                    }
                    dto.Icono = nuevaRuta;
                }
            }

            if (!ModelState.IsValid)
            {
                var categoriaOriginal = await repositorioAdmin.ObtenerCategoriaPorId(id);
                if (dto.ImagenFile == null)
                {
                    dto.Icono = categoriaOriginal.Icono;
                }
                return View("Categorias/Editar", dto);
            }

            await repositorioAdmin.ActualizarCategoria(id, dto);
            TempData["Mensaje"] = "Categoría actualizada exitosamente";
            return RedirectToAction(nameof(Categorias));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await repositorioAdmin.ObtenerCategoriaPorId(id);
            if (categoria == null)
            {
                TempData["Error"] = "La categoría no fue encontrada.";
                return RedirectToAction(nameof(Categorias));
            }

            await repositorioAdmin.EliminarCategoria(id);

            if (!string.IsNullOrEmpty(categoria.Icono))
            {
                await servicioArchivos.EliminarImagen(categoria.Icono);
            }

            TempData["Mensaje"] = "Categoría eliminada exitosamente";
            return RedirectToAction(nameof(Categorias));
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

        public IActionResult CrearRiego()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearRiego(Riego riego)
        {
            if (!ModelState.IsValid)
            {
                return View(riego);
            }

            await repositorioAdmin.CrearRiego(riego);
            TempData["Mensaje"] = "Tipo de riego creado exitosamente";
            return RedirectToAction(nameof(Riegos));
        }

        public async Task<IActionResult> EditarRiego(int id)
        {
            var riego = await repositorioAdmin.ObtenerRiegoPorId(id);
            if (riego == null) return NotFound();

            return View(riego);
        }

        [HttpPost]
        public async Task<IActionResult> EditarRiego(int id, Riego riego)
        {
            if (!ModelState.IsValid)
            {
                return View(riego);
            }

            await repositorioAdmin.ActualizarRiego(id, riego);
            TempData["Mensaje"] = "Tipo de riego actualizado exitosamente";
            return RedirectToAction(nameof(Riegos));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarRiego(int id)
        {
            await repositorioAdmin.EliminarRiego(id);
            TempData["Mensaje"] = "Tipo de riego eliminado exitosamente";
            return RedirectToAction(nameof(Riegos));
        }

        public async Task<IActionResult> Luces()
        {
            var luces = await repositorioAdmin.ObtenerLuces();
            return View(luces);
        }

        public IActionResult CrearLuz()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearLuz(Luz luz)
        {
            if (!ModelState.IsValid)
            {
                return View(luz);
            }

            await repositorioAdmin.CrearLuz(luz);
            TempData["Mensaje"] = "Tipo de luz creado exitosamente";
            return RedirectToAction(nameof(Luces));
        }

        public async Task<IActionResult> EditarLuz(int id)
        {
            var luz = await repositorioAdmin.ObtenerLuzPorId(id);
            if (luz == null) return NotFound();

            return View(luz);
        }

        [HttpPost]
        public async Task<IActionResult> EditarLuz(int id, Luz luz)
        {
            if (!ModelState.IsValid)
            {
                return View(luz);
            }

            await repositorioAdmin.ActualizarLuz(id, luz);
            TempData["Mensaje"] = "Tipo de luz actualizado exitosamente";
            return RedirectToAction(nameof(Luces));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarLuz(int id)
        {
            await repositorioAdmin.EliminarLuz(id);
            TempData["Mensaje"] = "Tipo de luz eliminado exitosamente";
            return RedirectToAction(nameof(Luces));
        }

        public async Task<IActionResult> Sustratos()
        {
            var sustratos = await repositorioAdmin.ObtenerSustratos();
            return View(sustratos);
        }

        public IActionResult CrearSustrato()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearSustrato(Sustrato sustrato)
        {
            if (!ModelState.IsValid)
            {
                return View(sustrato);
            }

            await repositorioAdmin.CrearSustrato(sustrato);
            TempData["Mensaje"] = "Tipo de sustrato creado exitosamente";
            return RedirectToAction(nameof(Sustratos));
        }

        public async Task<IActionResult> EditarSustrato(int id)
        {
            var sustrato = await repositorioAdmin.ObtenerSustratoPorId(id);
            if (sustrato == null) return NotFound();

            return View(sustrato);
        }

        [HttpPost]
        public async Task<IActionResult> EditarSustrato(int id, Sustrato sustrato)
        {
            if (!ModelState.IsValid)
            {
                return View(sustrato);
            }

            await repositorioAdmin.ActualizarSustrato(id, sustrato);
            TempData["Mensaje"] = "Tipo de sustrato actualizado exitosamente";
            return RedirectToAction(nameof(Sustratos));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSustrato(int id)
        {
            await repositorioAdmin.EliminarSustrato(id);
            TempData["Mensaje"] = "Tipo de sustrato eliminado exitosamente";
            return RedirectToAction(nameof(Sustratos));
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