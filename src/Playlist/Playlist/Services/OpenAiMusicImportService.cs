using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Playlist.ViewModels;

namespace Playlist.Services;

public sealed class OpenAiMusicImportService : IAiMusicImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiMusicImportService> _logger;

    public OpenAiMusicImportService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiMusicImportService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());

    public async Task<AiImportInterpretation> CreateImportDraftAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        var requestBody = new
        {
            model = _configuration["OpenAI:Model"] ?? "gpt-5.4-mini",
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = "Determine whether the user wants to add a Song, Artist or Album and create one draft. Do not invent missing facts: use Unknown for missing text, 0 for missing numbers and 2000-01-01 for missing dates. ReleaseDate must be ISO yyyy-MM-dd. Mood must be one of Happy, Sad, Energetic, Chill, Romantic, Angry or Focus."
                },
                new { role = "user", content = prompt }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "musicbar_entity_draft",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            entityType = new { type = "string", @enum = new[] { "Song", "Artist", "Album" } },
                            title = new { type = "string" },
                            artistName = new { type = "string" },
                            albumTitle = new { type = "string" },
                            genreName = new { type = "string" },
                            durationSeconds = new { type = "integer", minimum = 0, maximum = 7200 },
                            releaseDate = new { type = "string", format = "date" },
                            mood = new { type = "string", @enum = new[] { "Happy", "Sad", "Energetic", "Chill", "Romantic", "Angry", "Focus" } },
                            isExplicit = new { type = "boolean" }
                            ,country = new { type = "string" }
                            ,biography = new { type = "string" }
                            ,label = new { type = "string" }
                            ,totalTracks = new { type = "integer", minimum = 0, maximum = 500 }
                            ,isActive = new { type = "boolean" }
                        },
                        required = new[] { "entityType", "title", "artistName", "albumTitle", "genreName", "durationSeconds", "releaseDate", "mood", "isExplicit", "country", "biography", "label", "totalTracks", "isActive" },
                        additionalProperties = false
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation("Requesting an AI song draft from model {Model}.", requestBody.model);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI request failed with HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("AI service could not create a draft. Check the API key and try again.");
        }

        using var document = JsonDocument.Parse(responseJson);
        var outputText = document.RootElement
            .GetProperty("output")
            .EnumerateArray()
            .Where(item => item.TryGetProperty("type", out var type) && type.GetString() == "message")
            .SelectMany(item => item.GetProperty("content").EnumerateArray())
            .FirstOrDefault(item => item.TryGetProperty("type", out var type) && type.GetString() == "output_text");

        if (outputText.ValueKind == JsonValueKind.Undefined
            || !outputText.TryGetProperty("text", out var textElement))
        {
            throw new InvalidOperationException("AI service returned no structured song draft.");
        }

        return JsonSerializer.Deserialize<AiImportInterpretation>(textElement.GetString()!, JsonOptions)
            ?? throw new InvalidOperationException("AI import draft could not be read.");
    }

    private string? GetApiKey() =>
        _configuration["OpenAI:ApiKey"]
        ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
}
