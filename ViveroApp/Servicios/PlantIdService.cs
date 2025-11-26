using System.Text;
using System.Text.Json;
using ViveroApp.Dto;

namespace ViveroApp.Servicios
{
    public interface IPlantIdService
    {
        Task<PlantIdIdentificationResponse?> IdentificarPlanta(string imagenBase64, double? latitude = null, double? longitude = null);
    }

    public class PlantIdService : IPlantIdService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private const string API_BASE_URL = "https://plant.id/api/v3";

        public PlantIdService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiKey = configuration["PlantId:ApiKey"] ?? throw new InvalidOperationException("PlantId:ApiKey no configurada");
        }

        public async Task<PlantIdIdentificationResponse?> IdentificarPlanta(
            string imagenBase64,
            double? latitude = null,
            double? longitude = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Construir el request
                var request = new PlantIdIdentificationRequest
                {
                    Images = new List<string> { imagenBase64 },
                    Latitude = latitude ?? 29.0729, // Hermosillo por defecto
                    Longitude = longitude ?? -110.9559,
                    Similar_images = new List<string> { "url" }
                };

                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{API_BASE_URL}/identification")
                {
                    Content = content
                };
                httpRequest.Headers.Add("Api-Key", _apiKey);

                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<PlantIdIdentificationResponse>(responseContent, options);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error identificando planta: {ex.Message}");
                return null;
            }
        }
    }
}