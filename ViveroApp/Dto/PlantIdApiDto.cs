using System.Text.Json;
using System.Text.Json.Serialization;

namespace ViveroApp.Dto
{
    // DTOs para la API de Plant.id - Basados en respuesta real

    public class PlantIdIdentificationRequest
    {
        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class PlantIdIdentificationResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = string.Empty;

        [JsonPropertyName("custom_id")]
        public string? CustomId { get; set; }

        [JsonPropertyName("input")]
        public PlantIdInput Input { get; set; } = new();

        [JsonPropertyName("result")]
        public PlantIdResult Result { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("sla_compliant_client")]
        public bool SlaCompliantClient { get; set; }

        [JsonPropertyName("sla_compliant_system")]
        public bool SlaCompliantSystem { get; set; }

        [JsonPropertyName("created")]
        public double Created { get; set; }

        [JsonPropertyName("completed")]
        public double Completed { get; set; }
    }

    public class PlantIdInput
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("similar_images")]
        public bool SimilarImages { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();

        [JsonPropertyName("datetime")]
        public string? Datetime { get; set; }
    }

    public class PlantIdResult
    {
        [JsonPropertyName("is_plant")]
        public PlantIdIsPlant IsPlant { get; set; } = new();

        [JsonPropertyName("classification")]
        public PlantIdClassification Classification { get; set; } = new();

        // Helper property - ignorar en serialización
        [JsonIgnore]
        public bool IsPlantBinary => IsPlant?.Binary ?? false;
    }

    public class PlantIdIsPlant
    {
        [JsonPropertyName("probability")]
        public double Probability { get; set; }

        [JsonPropertyName("binary")]
        public bool Binary { get; set; }

        [JsonPropertyName("threshold")]
        public double Threshold { get; set; }
    }

    public class PlantIdClassification
    {
        [JsonPropertyName("suggestions")]
        public List<PlantIdSuggestion> Suggestions { get; set; } = new();
    }

    public class PlantIdSuggestion
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("probability")]
        public double Probability { get; set; }

        [JsonPropertyName("similar_images")]
        public List<PlantIdSimilarImage> SimilarImages { get; set; } = new();

        [JsonPropertyName("details")]
        public PlantIdDetails Details { get; set; } = new();
    }

    public class PlantIdSimilarImage
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("license_name")]
        public string? LicenseName { get; set; }

        [JsonPropertyName("license_url")]
        public string? LicenseUrl { get; set; }

        [JsonPropertyName("citation")]
        public string? Citation { get; set; }

        [JsonPropertyName("similarity")]
        public double Similarity { get; set; }

        [JsonPropertyName("url_small")]
        public string? UrlSmall { get; set; }
    }

   public class PlantIdDetails
{
    [JsonPropertyName("common_names")]
    public List<string>? CommonNames { get; set; }

    [JsonPropertyName("synonyms")]
    public List<string>? Synonyms { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("watering")]
    public PlantIdWatering? Watering { get; set; }

    // Agregar otras propiedades que pueden venir
    [JsonPropertyName("description")]
    public PlantIdDescription? Description { get; set; }

    [JsonPropertyName("taxonomy")]
    public PlantIdTaxonomy? Taxonomy { get; set; }

    [JsonPropertyName("image")]
    public PlantIdImage? Image { get; set; }
}

    public class PlantIdWatering
    {
        [JsonPropertyName("max")]
        [JsonConverter(typeof(WateringValueConverter))]
        public string Max { get; set; } = string.Empty;

        [JsonPropertyName("min")]
        [JsonConverter(typeof(WateringValueConverter))]
        public string Min { get; set; } = string.Empty;
    }

    // CONVERSOR PERSONALIZADO MEJORADO
    public class WateringValueConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        if (reader.TryGetInt32(out int intValue))
                            return intValue.ToString();
                        if (reader.TryGetInt64(out long longValue))
                            return longValue.ToString();
                        if (reader.TryGetDouble(out double doubleValue))
                            return doubleValue.ToString();
                        return reader.GetDecimal().ToString();

                    case JsonTokenType.String:
                        return reader.GetString() ?? string.Empty;

                    case JsonTokenType.True:
                        return "true";

                    case JsonTokenType.False:
                        return "false";

                    case JsonTokenType.Null:
                        return string.Empty;

                    default:
                        // Para cualquier otro tipo, intentar leer como string
                        return reader.GetString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en WateringValueConverter: {ex.Message}");
                return string.Empty;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }


    public class PlantIdDescription
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("citation")]
    public string? Citation { get; set; }
}

public class PlantIdTaxonomy
{
    [JsonPropertyName("class")]
    public string? Class { get; set; }

    [JsonPropertyName("genus")]
    public string? Genus { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }
}

public class PlantIdImage
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("citation")]
    public string? Citation { get; set; }
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