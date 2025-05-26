using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OnlineStore.Models;

namespace OnlineStore.Services;

public class RecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly string _recommendationApiUrl;

    public RecommendationService(HttpClient httpClient, IOptions<RecommendationOptions> options)
    {
        _httpClient = httpClient;
        _recommendationApiUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://model_server:8001";
    }

    public async Task<List<string>> GetRecommendations(string userId, int topK = 5)
    {
        try
        {
            var url = $"{_recommendationApiUrl}/recommend/{userId}?top_k={topK}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecommendationResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Recommendations ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при получении рекомендаций: {ex.Message}");
            return new List<string>();
        }
    }
}
