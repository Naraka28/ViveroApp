namespace ViveroApp.Dto
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }
        public string? Descripcion { get; set; }
    }
}
