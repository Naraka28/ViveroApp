using System.Numerics;

namespace ViveroApp.Models
{
    public class PlantaCategoria
    {
        public int PlantaId { get; set; }
        public virtual Plantas Planta { get; set; }

        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; }
    }
}