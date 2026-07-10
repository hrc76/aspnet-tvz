using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Playlist.Services;

namespace Playlist.Tests;

public sealed class MusicBrainzMetadataServiceTests
{
    // MusicBrainz JSON mora se pretvoriti u podatke koje AI draft razumije.
    [Fact]
    public async Task SearchRecordingsAsync_MapsVerifiedSongMetadata()
    {
        const string json = """
        {
          "recordings": [{
            "id": "abc-123",
            "score": 100,
            "title": "Smells Like Teen Spirit",
            "length": 301000,
            "first-release-date": "1991-09-10",
            "artist-credit": [{ "name": "Nirvana" }],
            "releases": [{ "title": "Nevermind", "date": "1991-09-24" }],
            "genres": [{ "name": "grunge", "count": 8 }]
          }]
        }
        """;
        var client = new HttpClient(new JsonResponseHandler(json))
        {
            BaseAddress = new Uri("https://musicbrainz.org/ws/2/")
        };
        var service = new MusicBrainzMetadataService(client, NullLogger<MusicBrainzMetadataService>.Instance);

        var results = await service.SearchRecordingsAsync("Smells Like Teen Spirit");

        results.Should().ContainSingle();
        var song = results[0];
        song.Title.Should().Be("Smells Like Teen Spirit");
        song.ArtistName.Should().Be("Nirvana");
        song.AlbumTitle.Should().Be("Nevermind");
        song.GenreName.Should().Be("grunge");
        song.DurationSeconds.Should().Be(301);
        song.ReleaseDate.Should().Be(new DateTime(1991, 9, 24));
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
