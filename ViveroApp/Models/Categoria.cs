using System.Collections.Generic;

namespace ViveroApp.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }

        public virtual ICollection<PlantaCategoria> PlantaCategorias { get; set; }
    }
}