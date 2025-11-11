namespace ViveroApp.Servicios
{
    public interface IServicioArchivos
    {
        Task<string> GuardarImagen(IFormFile archivo, string carpeta = "plantas");
        Task<bool> EliminarImagen(string rutaImagen);
        bool ValidarImagen(IFormFile archivo);
    }
    public class ServicioArchivos : IServicioArchivos
    {
        private readonly IWebHostEnvironment environment;
        private readonly string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long tamañoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ServicioArchivos(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<string> GuardarImagen(IFormFile archivo, string carpeta = "plantas")
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            if (!ValidarImagen(archivo))
                throw new Exception("Archivo de imagen inválido");

            var baseCarpetaImagenes = Path.Combine(environment.WebRootPath, "Images");

            var carpetaAssets = Path.Combine(baseCarpetaImagenes, carpeta);

            if (!Directory.Exists(carpetaAssets))
            {
                Directory.CreateDirectory(carpetaAssets);
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpetaAssets, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var urlBase = "/Images";

            if (!string.IsNullOrEmpty(carpeta))
            {
                var urlCarpeta = carpeta.Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
                return $"{urlBase}/{urlCarpeta}/{nombreArchivo}";
            }

            return $"{urlBase}/{nombreArchivo}";
        }

        public async Task<bool> EliminarImagen(string rutaImagen)
        {
            if (string.IsNullOrEmpty(rutaImagen))
                return false;

            try
            {
                var rutaFisica = Path.Combine(environment.WebRootPath, rutaImagen.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(rutaFisica))
                {
                    File.Delete(rutaFisica);
                    return true;
                }

                return false;
            }
            catch
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

            var tiposMimePermitidos = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!tiposMimePermitidos.Contains(archivo.ContentType.ToLower()))
                return false;

            return true;
        }
    }
}
