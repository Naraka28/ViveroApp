using System.Collections.Generic;
using System.Numerics;

namespace ViveroApp.Models
{
    public class Riego
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int? FrecuenciaDiasVerano { get; set; }
        public int? FrecuenciaDiasInvierno { get; set; }
        public string Nivel { get; set; }
        public string Recomendaciones { get; set; }

        public virtual ICollection<Plantas> Plantas { get; set; }
    }
}