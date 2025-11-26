namespace ViveroApp.Dto
{
    public class PlantasPorCategoriaDto
    {
        public string CategoriaNombre { get; set; }
        public string CategoriaDescripcion { get; set; }
        public IEnumerable<PlantaPopularDto> Plantas { get; set; }
    }
}
