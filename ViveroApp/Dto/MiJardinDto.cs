namespace ViveroApp.DTOs
{
    public class MiJardinDto
    {
        public int Mi_Jardin_Id { get; set; }
        public int Planta_Id { get; set; }
        public string Planta_Nombre { get; set; } = string.Empty;
        public string? Planta_Nombre_Cientifico { get; set; }
        public string? Planta_Imagen_Url { get; set; }
        public string? Ubicacion { get; set; }
        public string? Apodo { get; set; }
        public DateTime Fecha_Agregada { get; set; }
        public string? Riego_Nombre { get; set; }
        public int? Frecuencia_Dias_Verano { get; set; }
        public int? Frecuencia_Dias_Invierno { get; set; }
        public string? Luz_Tipo { get; set; }
        public string? Notas_Personales { get; set; }
        public DateTime? Ultimo_Riego { get; set; }
        public DateTime? Ultimo_Fertilizado { get; set; }
        public int? Orden { get; set; }
        public int MiJardinId => Mi_Jardin_Id;
        public int PlantaId => Planta_Id;
        public string PlantaNombre => Planta_Nombre;
        public string PlantaNombreCientifico => Planta_Nombre_Cientifico ?? "";
        public string PlantaImagenUrl => Planta_Imagen_Url ?? "";
        public string RiegoNombre => Riego_Nombre ?? "";
        public string LuzTipo => Luz_Tipo ?? "";
        public string NotasPersonales => Notas_Personales ?? "";
        public DateTime? UltimoRiego => Ultimo_Riego;
        public DateTime? UltimoFertilizado => Ultimo_Fertilizado;

        public bool NecesitaRiego
        {
            get
            {
                if (!Ultimo_Riego.HasValue || !Frecuencia_Dias_Verano.HasValue)
                    return false;

                var diasDesdeUltimoRiego = (DateTime.Now - Ultimo_Riego.Value).Days;
                return diasDesdeUltimoRiego >= Frecuencia_Dias_Verano.Value;
            }
        }

        public int? DiasDesdeUltimoRiego
        {
            get
            {
                if (!Ultimo_Riego.HasValue)
                    return null;

                return (DateTime.Now - Ultimo_Riego.Value).Days;
            }
        }
    }
}