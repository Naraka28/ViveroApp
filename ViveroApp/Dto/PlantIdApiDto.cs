namespace ViveroApp.Dto
{
    // DTOs para la API de Plant.id

    public class PlantIdIdentificationRequest
    {
        public List<string> Images { get; set; } = new(); // Base64 strings
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Similar_images { get; set; } = new();
    }

    public class PlantIdIdentificationResponse
    {
        public string Access_token { get; set; } = string.Empty;
        public string Model_version { get; set; } = string.Empty;
        public PlantIdInput Input { get; set; } = new();
        public PlantIdResult Result { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class PlantIdInput
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Similar_images { get; set; } = new();
    }

    public class PlantIdResult
    {
        public bool Is_plant { get; set; }
        public PlantIdClassification Classification { get; set; } = new();
    }

    public class PlantIdClassification
    {
        public List<PlantIdSuggestion> Suggestions { get; set; } = new();
    }

    public class PlantIdSuggestion
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Probability { get; set; }
        public List<string> Similar_images { get; set; } = new();
        public PlantIdDetails Details { get; set; } = new();
    }

    public class PlantIdDetails
    {
        public List<string> Common_names { get; set; } = new();
        public List<string> Synonyms { get; set; } = new();
        public string Language { get; set; } = string.Empty;
        public PlantIdWatering Watering { get; set; } = new();
    }

    public class PlantIdWatering
    {
        public string Max { get; set; } = string.Empty;
        public string Min { get; set; } = string.Empty;
    }

    // DTO para búsqueda por texto en BD local
    public class BusquedaPlantaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Nombre_Cientifico { get; set; }
        public string? Imagen_Url { get; set; }
        public string? Dificultad { get; set; }
    }
}