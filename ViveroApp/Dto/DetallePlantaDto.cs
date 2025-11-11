namespace ViveroApp.Dto
{
    public class DetallePlantaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Nombre_Cientifico { get; set; } 
        public string? Descripcion { get; set; }
        public string? Imagen_Url { get; set; }
        public string? Cuidados_Especiales { get; set; }
        public int? Altura_Max_Cm { get; set; }
        public string? Dificultad { get; set; }
        public bool Toxica { get; set; }
        public string? Notas { get; set; }

        public int RiegoId { get; set; }
        public string RiegoNombre { get; set; } = string.Empty;
        public string? RiegoDescripcion { get; set; }
        public int? Frecuencia_Dias_Verano { get; set; }
        public int? Frecuencia_Dias_Invierno { get; set; }
        public string? RiegoNivel { get; set; }
        public string? RiegoRecomendaciones { get; set; }

        public int LuzId { get; set; }
        public string LuzTipo { get; set; } = string.Empty;
        public string? LuzDescripcion { get; set; }
        public int? Horas_Min { get; set; }
        public int? Horas_Max { get; set; }
        public string? Ubicacion_Recomendada { get; set; }

        public int SustratoId { get; set; }
        public string SustratoTipo { get; set; } = string.Empty;
        public string? SustratoDescripcion { get; set; }
        public string? Ph_Rango { get; set; }
        public string? Drenaje { get; set; }
        public string? Composicion { get; set; }
        public List<CategoriaDto> Categorias { get; set; } = new();
        public string NombreCientifico => Nombre_Cientifico ?? "";
        public string ImagenUrl => Imagen_Url ?? "";
        public string CuidadosEspeciales => Cuidados_Especiales ?? "";
        public int? AlturaMaxCm => Altura_Max_Cm;
        public int? FrecuenciaDiasVerano => Frecuencia_Dias_Verano;
        public int? FrecuenciaDiasInvierno => Frecuencia_Dias_Invierno;
        public int? HorasMin => Horas_Min;
        public int? HorasMax => Horas_Max;
        public string UbicacionRecomendada => Ubicacion_Recomendada ?? "";
        public string PhRango => Ph_Rango ?? "";
    }
}
