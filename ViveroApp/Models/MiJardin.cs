using System;
using System.Numerics;

namespace ViveroApp.Models
{
    public class MiJardin
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PlantaId { get; set; }
        public DateTime FechaAgregada { get; set; }
        public string NotasPersonales { get; set; }
        public DateTime? FechaAdquisicion { get; set; }
        public string UbicacionCasa { get; set; }
        public DateTime? UltimoRiego { get; set; }
        public DateTime? UltimoFertilizado { get; set; }
        public bool NotificacionesRiego { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Plantas Planta { get; set; }
    }
}