using System.ComponentModel.DataAnnotations;

namespace ViveroApp.Dto
{
    public class EditarPerfilDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;
    }
}
