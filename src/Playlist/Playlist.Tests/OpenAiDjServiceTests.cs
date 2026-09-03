using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Playlist.Services;
using Playlist.ViewModels;

namespace Playlist.Tests;

public sealed class OpenAiDjServiceTests
{
    // AI DJ smije vratiti samo jedinstvene ID-eve pjesama koje smo mu ponudili.
    [Fact]
    public async Task RecommendAsync_RemovesUnknownAndDuplicateSongIds()
    {
        const string recommendation = """
        {"playlistName":"Workout Fire","description":"Fast tracks for training.","explanation":"High energy and variety.","songIds":[2,999,2,1]}
        """;
        var handler = new JsonResponseHandler(WrapResponse(recommendation));
        var service = CreateService(handler);
        var catalog = new[]
        {
            Song(1, "One", "Rock"),
            Song(2, "Two", "Techno")
        };

        var result = await service.RecommendAsync("Workout mix", catalog, new AiDjListenerProfile());

        result.SongIds.Should().Equal(2, 1);
        result.PlaylistName.Should().Be("Workout Fire");
    }

    // Zbog privatnosti OpenAI zahtjev ne smije spremati response na platformi.
    [Fact]
    public async Task RecommendAsync_DisablesResponseStorage()
    {
        const string recommendation = """
        {"playlistName":"Mix","description":"A useful personalized mix.","explanation":"Matches the request.","songIds":[1]}
        """;
        var handler = new JsonResponseHandler(WrapResponse(recommendation));
        var service = CreateService(handler);

        await service.RecommendAsync("Surprise me", new[] { Song(1, "One", "Rock") }, new AiDjListenerProfile());

        using var body = JsonDocument.Parse(handler.RequestBody!);
        body.RootElement.GetProperty("store").GetBoolean().Should().BeFalse();
    }

    private static OpenAiDjService CreateService(JsonResponseHandler handler)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["OpenAI:ApiKey"] = "test-key" }).Build();
        return new OpenAiDjService(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.openai.com/v1/") },
            configuration,
            NullLogger<OpenAiDjService>.Instance);
    }

    private static AiDjCatalogSong Song(int id, string title, string genre) => new()
    {
        SongId = id, Title = title, Artist = "Artist", Album = "Album", Genre = genre,
        Mood = "Energetic", DurationSeconds = 200, Popularity = 8
    };

    private static string WrapResponse(string output) => JsonSerializer.Serialize(new
    {
        output = new[] { new { type = "message", content = new[] { new { type = "output_text", text = output } } } }
    });

    private sealed class JsonResponseHandler : HttpMessageHandler
    {
        private readonly string _response;
        public string? RequestBody { get; private set; }
        public JsonResponseHandler(string response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response, Encoding.UTF8, "application/json")
            };
        }
    }
}
