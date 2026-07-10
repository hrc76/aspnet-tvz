using System.Text.Json;
using Playlist.ViewModels;

namespace Playlist.Services;

public sealed class MusicBrainzMetadataService : IMusicMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MusicBrainzMetadataService> _logger;

    public MusicBrainzMetadataService(HttpClient httpClient, ILogger<MusicBrainzMetadataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MusicMetadataCandidate>> SearchRecordingsAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title)) return Array.Empty<MusicMetadataCandidate>();

        var url = $"recording/?query={Uri.EscapeDataString(title.Trim())}&fmt=json&limit=5";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("MusicBrainz search failed with HTTP {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("Music metadata search is temporarily unavailable.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("recordings", out var recordings))
            return Array.Empty<MusicMetadataCandidate>();

        var results = new List<MusicMetadataCandidate>();
        foreach (var recording in recordings.EnumerateArray())
        {
            var artist = ReadArtist(recording);
            var (album, releaseDate) = ReadRelease(recording);
            var lengthMs = ReadInt(recording, "length");
            var firstReleaseDate = ReadString(recording, "first-release-date");
            if (releaseDate == null && DateTime.TryParse(firstReleaseDate, out var parsedDate)) releaseDate = parsedDate;

            results.Add(new MusicMetadataCandidate
            {
                MusicBrainzId = ReadString(recording, "id"),
                Title = ReadString(recording, "title"),
                ArtistName = artist,
                AlbumTitle = album,
                GenreName = ReadGenre(recording),
                DurationSeconds = lengthMs > 0 ? (int)Math.Round(lengthMs / 1000d) : 0,
                ReleaseDate = releaseDate
            });
        }

        _logger.LogInformation("MusicBrainz returned {Count} candidates for {Title}.", results.Count, title);
        return results;
    }

    private static string ReadArtist(JsonElement recording)
    {
        if (!recording.TryGetProperty("artist-credit", out var credits)) return string.Empty;
        var names = credits.EnumerateArray()
            .Select(credit => ReadString(credit, "name"))
            .Where(name => !string.IsNullOrWhiteSpace(name));
        return string.Join(", ", names);
    }

    private static (string Album, DateTime? Date) ReadRelease(JsonElement recording)
    {
        if (!recording.TryGetProperty("releases", out var releases)) return (string.Empty, null);
        var release = releases.EnumerateArray().FirstOrDefault();
        if (release.ValueKind == JsonValueKind.Undefined) return (string.Empty, null);
        DateTime? date = DateTime.TryParse(ReadString(release, "date"), out var parsed) ? parsed : null;
        return (ReadString(release, "title"), date);
    }

    private static string ReadGenre(JsonElement recording)
    {
        var property = recording.TryGetProperty("genres", out var genres) ? genres
            : recording.TryGetProperty("tags", out var tags) ? tags
            : default;
        if (property.ValueKind != JsonValueKind.Array) return string.Empty;

        return property.EnumerateArray()
            .OrderByDescending(item => ReadInt(item, "count"))
            .Select(item => ReadString(item, "name"))
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty;
    }

    private static string ReadString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int ReadInt(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.TryGetInt32(out var result) ? result : 0;
}
