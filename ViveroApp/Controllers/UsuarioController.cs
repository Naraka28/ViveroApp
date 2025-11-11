// ============================================
// CONTROLADOR DE PERFIL DE USUARIO
// ============================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ViveroApp.Dto;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    [Authorize] // ← Requiere autenticación para toda la clase
    public class UsuarioController : Controller
    {
        private readonly IRepositorioUsuarios repositorioUsuario;
        private readonly IRepositorioAutenticar repositorioAutenticar;
        private readonly IRepositorioMiJardin repositorioMiJardin;

        public UsuarioController(IRepositorioUsuarios usuarioRepository, IRepositorioAutenticar authService, IRepositorioMiJardin miJardinRepository)
        {
            repositorioUsuario = usuarioRepository;
            repositorioAutenticar = authService;
            repositorioMiJardin = miJardinRepository;
        }

        // GET: /Usuario/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            Models.Usuario usuario = await repositorioAutenticar.ObtenerUsuario();

            if (usuario == null)
                return RedirectToAction("Login", "Auth");
            var viewModel = new PerfilUsuarioViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                FechaRegistro = usuario.FechaRegistro,
                UltimoAcceso = usuario.UltimoAcceso,
                Activo = usuario.Activo,
                Rol = usuario.Rol,
                MiJardin = await repositorioMiJardin.ObtenerPlantasUsuario(usuario.Id)
            };

            return View(viewModel);
        }

        // GET: /Usuario/Editar
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var usuario = await repositorioAutenticar.ObtenerUsuario();

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            var model = new EditarPerfilDto
            {
                Nombre = usuario.Nombre,
                Email = usuario.Email
            };

            return View(model);
        }

        // POST: /Usuario/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPerfilDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await repositorioAutenticar.ObtenerUsuario();

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                await repositorioUsuario.ActualizarPerfil(usuario.Id, model.Nombre, model.Email);

                TempData["Mensaje"] = "Perfil actualizado exitosamente";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar el perfil: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Usuario/CambiarPassword
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        // POST: /Usuario/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await repositorioAutenticar.ObtenerUsuario();

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            var success = await repositorioAutenticar.CambiarContrasena(
                usuario.Id,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "La contraseña actual es incorrecta");
                return View(model);
            }

            TempData["Mensaje"] = "Contraseña cambiada exitosamente";
            return RedirectToAction(nameof(Perfil));
        }

        // POST: /Usuario/DesactivarCuenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCuenta()
        {
            var usuario = await repositorioAutenticar.ObtenerUsuario();

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            await repositorioUsuario.DesactivarCuenta(usuario.Id);
            await repositorioAutenticar.Logout();

            TempData["Mensaje"] = "Tu cuenta ha sido desactivada";
            return RedirectToAction("Index", "Home");
        }
    }
}