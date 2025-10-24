using System.Collections.Generic;
using System.Numerics;

namespace ViveroApp.Models
{
    public class Sustrato
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public string PhRango { get; set; }
        public string Drenaje { get; set; }
        public string Composicion { get; set; }
        public string Recomendaciones { get; set; }

        public virtual ICollection<Plantas> Plantas { get; set; }
    }
}