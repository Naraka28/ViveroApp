﻿using Microsoft.AspNetCore.Authorization;
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
        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
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

        // POST: /Auth/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await repositorioAutenticar.Logout();
            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Index", "Home");
        }
    }
}
