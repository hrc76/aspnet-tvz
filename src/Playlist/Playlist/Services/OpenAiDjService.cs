using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Playlist.ViewModels;

namespace Playlist.Services;

public sealed class OpenAiDjService : IAiDjService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiDjService> _logger;

    public OpenAiDjService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiDjService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());

    public async Task<AiDjRecommendation> RecommendAsync(
        string request,
        IReadOnlyCollection<AiDjCatalogSong> catalog,
        AiDjListenerProfile profile,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured.");
        if (catalog.Count == 0)
            throw new InvalidOperationException("The music catalog is empty.");

        // Whitelist sprjecava da model izmisli pjesmu koja ne postoji u MusicBar bazi.
        var allowedIds = catalog.Select(song => song.SongId).ToHashSet();
        var context = JsonSerializer.Serialize(new { listener = profile, availableSongs = catalog });
        // JSON schema prisiljava model da vrati strojno citljiv rezultat umjesto slobodnog teksta.
        var body = new
        {
            model = _configuration["OpenAI:Model"] ?? "gpt-5-mini",
            store = false,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are MusicBar AI DJ. Build a coherent personalized playlist for the request. You may ONLY select songIds from availableSongs. Use favorites, saved albums and listening counts as soft preferences, but honor the current request first. Prefer variety unless the user asks otherwise. Return 5-10 unique songs when the catalog permits. Keep playlistName under 80 characters, description under 250 characters, and explain the selection briefly."
                },
                new { role = "user", content = $"Request: {request}\nMusicBar data: {context}" }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "musicbar_ai_dj_recommendation",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            playlistName = new { type = "string" },
                            description = new { type = "string" },
                            explanation = new { type = "string" },
                            songIds = new { type = "array", items = new { type = "integer" } }
                        },
                        required = new[] { "playlistName", "description", "explanation", "songIds" },
                        additionalProperties = false
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "responses");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        _logger.LogInformation("Requesting an AI DJ mix from {Model} using {SongCount} catalog songs.", body.model, catalog.Count);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI AI DJ request failed with HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("AI DJ could not create a mix. Check the API configuration and try again.");
        }

        // Responses API vraca niz output elemenata; trazimo tekst prve message poruke.
        using var document = JsonDocument.Parse(responseJson);
        var outputText = document.RootElement.GetProperty("output").EnumerateArray()
            .Where(item => item.TryGetProperty("type", out var type) && type.GetString() == "message")
            .SelectMany(item => item.GetProperty("content").EnumerateArray())
            .FirstOrDefault(item => item.TryGetProperty("type", out var type) && type.GetString() == "output_text");
        if (outputText.ValueKind == JsonValueKind.Undefined || !outputText.TryGetProperty("text", out var text))
            throw new InvalidOperationException("AI DJ returned no structured recommendation.");

        var result = JsonSerializer.Deserialize<AiDjRecommendation>(text.GetString()!, JsonOptions)
            ?? throw new InvalidOperationException("AI DJ recommendation could not be read.");
        // Posljednja provjera je na serveru, neovisno o tome sto je AI vratio.
        result.SongIds = result.SongIds.Where(allowedIds.Contains).Distinct().Take(12).ToList();
        if (result.SongIds.Count == 0)
            throw new InvalidOperationException("AI DJ did not select any songs from the MusicBar catalog.");
        result.PlaylistName = Normalize(result.PlaylistName, "AI Mix", 80, 2);
        result.Description = Normalize(result.Description, "A personalized AI DJ selection.", 250, 5);
        result.Explanation = Normalize(result.Explanation, "Selected from your MusicBar catalog.", 500, 1);
        return result;
    }

    private static string Normalize(string? value, string fallback, int maxLength, int minimumLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) || value.Trim().Length < minimumLength ? fallback : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength].TrimEnd();
    }

    private string? GetApiKey() => _configuration["OpenAI:ApiKey"]
        ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
}
