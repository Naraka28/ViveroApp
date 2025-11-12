using System;
using System.Collections.Generic;

namespace ViveroApp.Models
{
    public class Plantas
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Nombre_Cientifico { get; set; }
        public string Descripcion { get; set; }
        public string Imagen_Url { get; set; }
        public int Riego_Id { get; set; }
        public int Luz_Id { get; set; }
        public int Sustrato_Id { get; set; }
        public string Cuidados_Especiales { get; set; }
        public int? Altura_Max_Cm { get; set; }
        public string Dificultad { get; set; }
        public bool Toxica { get; set; }
        public string Notas { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }


        public virtual Riego Riego { get; set; }
        public virtual Luz Luz { get; set; }
        public virtual Sustrato Sustrato { get; set; }

        public virtual ICollection<PlantaCategoria> PlantaCategorias { get; set; }
        public virtual ICollection<MiJardin> MiJardin { get; set; }
    }
}