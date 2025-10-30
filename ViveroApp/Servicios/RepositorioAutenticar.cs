using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ViveroApp.Models;
using ViveroApp.Dto;

namespace ViveroApp.Servicios
{
    public interface IRepositorioAutenticar
    {
        Task<(bool Success, string Message, Usuario? Usuario)> Login(string email, string password, bool rememberMe);
        Task<(bool Success, string Message, int? UsuarioId)> Registrar(RegistroDto dto);
        Task Logout();
        Task<Usuario?> ObtenerUsuario();
        Task<bool> CambiarContrasena(int usuarioId, string currentPassword, string newPassword);
    }
    public class RepositorioAutenticar : IRepositorioAutenticar
    {
        private readonly IRepositorioUsuarios repositorioUsuario;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RepositorioAutenticar(IRepositorioUsuarios repositorioUsuario, IHttpContextAccessor _httpContextAccessor)
        {
            this.repositorioUsuario = repositorioUsuario;
            this._httpContextAccessor = _httpContextAccessor;
        }

        public async Task<(bool Success, string Message, Usuario? Usuario)> Login(
            string email,
            string password,
            bool rememberMe)
        {
            var usuario = await repositorioUsuario.ObtenerPorEmail(email);

            if (usuario == null)
                return (false, "Email o contraseña incorrectos", null);

            if (!usuario.Activo)
                return (false, "Tu cuenta está desactivada. Contacta al administrador.", null);

            bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.Password);

            if (!passwordValida)
                return (false, "Email o contraseña incorrectos", null);

            await repositorioUsuario.ActualizarUltimoAcceso(usuario.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("FechaRegistro", usuario.FechaRegistro.ToString("o"))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(2)
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties
            );

            return (true, "Inicio de sesión exitoso", usuario);
        }
        public async Task<(bool Success, string Message, int? UsuarioId)> Registrar(RegistroDto dto)
        {
            var usuarioExistente = await repositorioUsuario.ObtenerPorEmail(dto.Email);

            if (usuarioExistente != null)
                return (false, "El email ya está registrado", null);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var resultado = await repositorioUsuario.Crear(new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = passwordHash,
                FechaRegistro = DateTime.Now,
                Activo = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            if (resultado.Success)
                return (true, "Registro exitoso", resultado.UsuarioId);

            return (false, "Error al registrar usuario", null);
        }
        public async Task Logout()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }

        public Task<Usuario> ObtenerUsuario()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CambiarContrasena(int usuarioId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
