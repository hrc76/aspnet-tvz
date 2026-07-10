using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Playlist.Services;

namespace Playlist.Tests;

public sealed class OpenAiMusicImportServiceTests
{
    // AI odgovor za artista mora ostati artist draft, a Unknown vrijednosti su dozvoljene.
    [Fact]
    public async Task CreateImportDraftAsync_ReadsArtistContext()
    {
        const string draftJson = """
        {"entityType":"Artist","title":"Nirvana","artistName":"Nirvana","albumTitle":"Unknown","genreName":"Grunge","durationSeconds":0,"releaseDate":"1987-01-01","mood":"Energetic","isExplicit":false,"country":"USA","biography":"Unknown","label":"Unknown","totalTracks":0,"isActive":false}
        """;
        var escapedDraft = System.Text.Json.JsonSerializer.Serialize(draftJson);
        var responseJson = "{\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":" + escapedDraft + "}]}]}";
        var client = new HttpClient(new JsonResponseHandler(responseJson))
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenAI:ApiKey"] = "test-key" })
            .Build();
        var service = new OpenAiMusicImportService(
            client,
            configuration,
            NullLogger<OpenAiMusicImportService>.Instance);

        var result = await service.CreateImportDraftAsync("Dodaj artista Nirvana");

        result.EntityType.Should().Be("Artist");
        result.ArtistName.Should().Be("Nirvana");
        result.Country.Should().Be("USA");
        result.DurationSeconds.Should().Be(0);
    }

    private sealed class JsonResponseHandler : HttpMessageHandler
    {
        private readonly string _json;
        public JsonResponseHandler(string json) => _json = json;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json")
        });
    }
}
