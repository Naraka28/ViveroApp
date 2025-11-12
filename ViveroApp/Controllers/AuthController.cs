using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViveroApp.Dto;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IRepositorioAutenticar repositorioAutenticar;

        public AuthController(IRepositorioAutenticar repositorioAutenticar)
        {
            this.repositorioAutenticar = repositorioAutenticar;
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var (success, message, usuario) = await repositorioAutenticar.Login(
                model.Email,
                model.Password,
                model.RememberMe
            );

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Mensaje"] = $"¡Bienvenido {usuario!.Nombre}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistroDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message, usuarioId) = await repositorioAutenticar.Registrar(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Mensaje"] = "Registro exitoso. Ahora puedes iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await repositorioAutenticar.Logout();
            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Index", "Home");
        }
        //[HttpGet]
        //[Authorize]
        //public IActionResult CambiarContrasena()
        //{
        //    return View();
        //}

        //// POST: /Auth/CambiarContrasena
        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CambiarContrasena(CambiarContrasenaDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var usuario = await repositorioAutenticar.ObtenerUsuario();

        //    if (usuario == null)
        //        return RedirectToAction(nameof(Login));

        //    var success = await repositorioAutenticar.CambiarContrasena(
        //        usuario.Id,
        //        model.CurrentPassword,
        //        model.NewPassword
        //    );

        //    if (!success)
        //    {
        //        ModelState.AddModelError(string.Empty, "La contraseña actual es incorrecta");
        //        return View(model);
        //    }

        //    TempData["Mensaje"] = "Contraseña cambiada exitosamente";
        //    return RedirectToAction("Perfil", "Usuario");
        //}
    }
}