using ViveroApp.DTOs;

namespace ViveroApp.Dto
{
    public class PerfilUsuarioViewModel
    {
        // Datos del usuario
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public string Rol { get; set; } = string.Empty;

        // Plantas del jardín (usa el DTO)
        public IEnumerable<MiJardinDto> MiJardin { get; set; } = new List<MiJardinDto>();

        // Propiedades calculadas
        public int TotalPlantas => MiJardin?.Count() ?? 0;
        public int DiasComoMiembro => (DateTime.Now - FechaRegistro).Days;
    }
}
