using System.ComponentModel.DataAnnotations.Schema;
namespace ViveroApp.Dto
{
    public class PlantaPopularDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreCientifico { get; set; }
        public string? ImagenUrl { get; set; }
        public string Dificultad { get; set; } = string.Empty;
        [Column("usuarios_que_la_tienen")]
        public int UsuariosQueLaTienen { get; set; }
        public string TipoRiego { get; set; } = string.Empty;
        public string TipoLuz { get; set; } = string.Empty;
    }
}
