using System;
using System.Collections.Generic;

namespace ViveroApp.Models
{
    public class Plantas
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NombreCientifico { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public int RiegoId { get; set; }
        public int LuzId { get; set; }
        public int SustratoId { get; set; }
        public string CuidadosEspeciales { get; set; }
        public int? AlturaMaxCm { get; set; }
        public string Dificultad { get; set; }
        public bool Toxica { get; set; }
        public string Notas { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public virtual Riego Riego { get; set; }
        public virtual Luz Luz { get; set; }
        public virtual Sustrato Sustrato { get; set; }

        public virtual ICollection<PlantaCategoria> PlantaCategorias { get; set; }
        public virtual ICollection<MiJardin> MiJardin { get; set; }
    }
}