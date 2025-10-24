using System.Collections.Generic;
using System.Numerics;

namespace ViveroApp.Models
{
    public class Luz
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public int? HorasMin { get; set; }
        public int? HorasMax { get; set; }
        public string UbicacionRecomendada { get; set; }

        public virtual ICollection<Plantas> Plantas { get; set; }
    }
}