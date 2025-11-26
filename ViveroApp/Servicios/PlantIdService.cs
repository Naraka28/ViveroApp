using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        // Configuración de JSON consistente
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

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
                client.Timeout = TimeSpan.FromSeconds(30);

                // Paso 1: Identificar la planta
                var identificationResponse = await EnviarIdentificacion(client, imagenBase64, latitude, longitude);

                if (identificationResponse == null || string.IsNullOrEmpty(identificationResponse.AccessToken))
                {
                    Console.WriteLine("❌ No se obtuvo access_token en la identificación");
                    return null;
                }

                Console.WriteLine($"✅ Identificación exitosa. Access token: {identificationResponse.AccessToken}");

                // Esperar un momento antes de obtener detalles
                await Task.Delay(2000);

                // Paso 2: Obtener detalles con el access_token
                var detallesResponse = await ObtenerDetalles(client, identificationResponse.AccessToken);

                return detallesResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error identificando planta: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private async Task<PlantIdIdentificationResponse?> EnviarIdentificacion(
            HttpClient client,
            string imagenBase64,
            double? latitude,
            double? longitude)
        {
            try
            {
                // Limpiar Base64
                if (imagenBase64.Contains(","))
                {
                    imagenBase64 = imagenBase64.Split(',')[1];
                }

                Console.WriteLine($"📸 Enviando imagen, Base64 length: {imagenBase64.Length}");

                // Construir request
                var request = new
                {
                    images = new[] { imagenBase64 },
                    latitude = latitude ?? 29.0729,
                    longitude = longitude ?? -110.9559,
                    similar_images = true
                };

                // Usar opciones de JSON para el request (snake_case para la API)
                var requestJsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonContent = JsonSerializer.Serialize(request, requestJsonOptions);
                Console.WriteLine($"📤 Request body preparado: {jsonContent.Length} caracteres");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{API_BASE_URL}/identification")
                {
                    Content = content
                };

                httpRequest.Headers.Add("Api-Key", _apiKey);

                Console.WriteLine($"🚀 Enviando request a: {API_BASE_URL}/identification");

                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error en la API: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Error en la API: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Respuesta recibida: {responseContent.Length} caracteres");
                Console.WriteLine($"📥 Respuesta (primeros 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}...");

                // Deserializar usando nuestras opciones con camelCase
                var result = JsonSerializer.Deserialize<PlantIdIdentificationResponse>(responseContent, _jsonOptions);

                if (result != null)
                {
                    Console.WriteLine($"✅ Identificación procesada. Status: {result.Status}, Access Token: {result.AccessToken}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en EnviarIdentificacion: {ex.Message}");
                throw;
            }
        }

        private async Task<PlantIdIdentificationResponse?> ObtenerDetalles(HttpClient client, string accessToken)
        {
            try
            {
                var url = $"{API_BASE_URL}/identification/{accessToken}?details=common_names,synonyms,watering&language=es";
                Console.WriteLine($"📤 Obteniendo detalles de: {url}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Add("Api-Key", _apiKey);

                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error obteniendo detalles: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Detalles recibidos: {responseContent.Length} caracteres");

                // Debug: Ver la estructura completa para entender qué viene
                Console.WriteLine($"📥 Respuesta completa de detalles:");
                Console.WriteLine(responseContent);

                var result = JsonSerializer.Deserialize<PlantIdIdentificationResponse>(responseContent, _jsonOptions);

                if (result != null)
                {
                    Console.WriteLine($"✅ Detalles obtenidos. Status: {result.Status}, Sugerencias: {result.Result?.Classification?.Suggestions?.Count ?? 0}");

                    // Debug: Ver qué detalles vienen realmente
                    if (result.Result?.Classification?.Suggestions?.Count > 0)
                    {
                        var suggestion = result.Result.Classification.Suggestions[0];
                        Console.WriteLine($"🔍 Detalles disponibles en la sugerencia:");
                        Console.WriteLine($"   - CommonNames: {suggestion.Details?.CommonNames?.Count ?? 0}");
                        Console.WriteLine($"   - Synonyms: {suggestion.Details?.Synonyms?.Count ?? 0}");
                        Console.WriteLine($"   - Watering: {(suggestion.Details?.Watering != null ? "Sí" : "No")}");
                        Console.WriteLine($"   - Description: {(suggestion.Details?.Description != null ? "Sí" : "No")}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo detalles: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}