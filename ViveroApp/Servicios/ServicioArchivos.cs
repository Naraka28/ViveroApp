using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ViveroApp.Servicios
{
    public interface IServicioArchivos
    {
        Task<string> GuardarImagen(IFormFile archivo, string carpeta);
        Task<bool> EliminarImagen(string rutaImagen);
        bool ValidarImagen(IFormFile archivo);
    }

    public class ServicioArchivos : IServicioArchivos
    {
        private readonly IWebHostEnvironment environment;
        private readonly string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp",".svg" };
        private readonly long tamañoMaximoBytes = 5 * 1024 * 1024;

        public ServicioArchivos(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<string> GuardarImagen(IFormFile archivo, string carpeta)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            if (!ValidarImagen(archivo))
                throw new Exception("Archivo de imagen inválido");

            if (string.IsNullOrWhiteSpace(carpeta))
                throw new ArgumentException("La carpeta es requerida", nameof(carpeta));

            carpeta = carpeta.Trim().ToLower();

            var baseCarpetaImagenes = Path.Combine(environment.WebRootPath, "images");
            var carpetaDestino = Path.Combine(baseCarpetaImagenes, carpeta);

            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/images/{carpeta}/{nombreArchivo}";
        }

        public async Task<bool> EliminarImagen(string rutaImagen)
        {
            if (string.IsNullOrEmpty(rutaImagen))
                return false;

            try
            {
                var rutaFisica = Path.Combine(
                    environment.WebRootPath,
                    rutaImagen.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (File.Exists(rutaFisica))
                {
                    await Task.Run(() => File.Delete(rutaFisica));
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidarImagen(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return false;

            if (archivo.Length > tamañoMaximoBytes)
                return false;

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(extension))
                return false;

            var tiposMimePermitidos = new[] {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/svg+xml"
            };

            if (!tiposMimePermitidos.Contains(archivo.ContentType.ToLower()))
                return false;

            return true;
        }
    }
}