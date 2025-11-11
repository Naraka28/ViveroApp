using System.ComponentModel.DataAnnotations;

namespace ViveroApp.Dto
{
    public class PlantaAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? NombreCientifico { get; set; }
        public string? ImagenUrl { get; set; }
        public string Dificultad { get; set; }
        public bool Toxica { get; set; }

        public string? RiegoNombre { get; set; }
        public string? LuzTipo { get; set; }
        public string? SustratoTipo { get; set; }
    }
    public class EstadisticasDto
    {
        public int TotalPlantas { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalCategorias { get; set; }
        public int PlantasEnJardines { get; set; }
    }

    public class CrearPlantaDto
    {
        [Required]
        public string Nombre { get; set; }
        public string? NombreCientifico { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }

        [Required]
        public int RiegoId { get; set; }

        [Required]
        public int LuzId { get; set; }

        [Required]
        public int SustratoId { get; set; }

        public string? CuidadosEspeciales { get; set; }
        public int? AlturaMaxCm { get; set; }
        public string Dificultad { get; set; } = "media";
        public bool Toxica { get; set; }
        public string? Notas { get; set; }
    }

    public class EditarPlantaDto : CrearPlantaDto
    {
        public int Id { get; set; }
    }

    public class UsuarioAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "usuario";

        public bool Activo { get; set; } = true;
    }
    public class EditarUsuarioDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; }

        public bool Activo { get; set; }
    }
}