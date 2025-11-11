using System.ComponentModel.DataAnnotations;

namespace ViveroApp.Dto
{
    public class CategoriaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        [StringLength(200, ErrorMessage = "El icono no puede exceder 200 caracteres")]
        public string Icono { get; set; }

        public IFormFile ImagenFile { get; set; }
    }
}
