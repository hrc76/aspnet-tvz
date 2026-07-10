using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playlist.Data;
using Playlist.Models;
using Playlist.Services;
using Playlist.ViewModels;

namespace Playlist.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class AiImportController : Controller
{
    private readonly IAiMusicImportService _ai;
    private readonly IMusicMetadataService _metadata;
    private readonly MusicBarDbContext _context;
    private readonly ILogger<AiImportController> _logger;

    public AiImportController(IAiMusicImportService ai, IMusicMetadataService metadata, MusicBarDbContext context, ILogger<AiImportController> logger)
    {
        _ai = ai;
        _metadata = metadata;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View(NewViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(AiMusicImportViewModel model, CancellationToken cancellationToken)
    {
        model.ApiConfigured = _ai.IsConfigured;
        model.GenreNames = GetGenreNames();
        if (!ModelState.IsValid) return View("Index", model);

        try
        {
            var interpreted = await _ai.CreateImportDraftAsync(model.Prompt, cancellationToken);
            var releaseDate = SafeDate(interpreted.ReleaseDate);
            switch (interpreted.EntityType.ToLowerInvariant())
            {
                case "artist":
                    model.ArtistDraft = new AiArtistDraft
                    {
                        StageName = KnownOr(interpreted.ArtistName, interpreted.Title, "Unknown artist"),
                        Country = KnownOr(interpreted.Country, "Unknown"),
                        DebutDate = releaseDate,
                        Biography = KnownOr(interpreted.Biography, "Unknown"),
                        IsActive = interpreted.IsActive
                    };
                    break;
                case "album":
                    model.AlbumDraft = new AiAlbumDraft
                    {
                        Title = KnownOr(interpreted.AlbumTitle, interpreted.Title, "Unknown album"),
                        ArtistName = KnownOr(interpreted.ArtistName, "Unknown artist"),
                        ReleaseDate = releaseDate,
                        Label = KnownOr(interpreted.Label, "Unknown"),
                        TotalTracks = Math.Clamp(interpreted.TotalTracks, 0, 500)
                    };
                    break;
                default:
                    model.Draft = new AiSongDraft
                    {
                        Title = KnownOr(interpreted.Title, "Unknown song"),
                        ArtistName = KnownOr(interpreted.ArtistName, "Unknown artist"),
                        AlbumTitle = KnownOr(interpreted.AlbumTitle, "Unknown album"),
                        GenreName = ResolveGenreName(interpreted.GenreName),
                        DurationSeconds = interpreted.DurationSeconds > 0 ? interpreted.DurationSeconds : 1,
                        ReleaseDate = releaseDate,
                        Mood = KnownOr(interpreted.Mood, "Energetic"),
                        IsExplicit = interpreted.IsExplicit
                    };
                    model.MetadataCandidates = (await _metadata.SearchRecordingsAsync(model.Draft.Title, cancellationToken)).ToList();
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(499);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "AI song draft generation failed.");
            model.ErrorMessage = exception.Message;
        }

        return View("Index", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SelectMetadata(MusicMetadataCandidate candidate)
    {
        var draft = new AiSongDraft
        {
            Title = candidate.Title.Trim(),
            ArtistName = candidate.ArtistName.Trim(),
            AlbumTitle = string.IsNullOrWhiteSpace(candidate.AlbumTitle) ? "Unknown album" : candidate.AlbumTitle.Trim(),
            GenreName = string.IsNullOrWhiteSpace(candidate.GenreName) ? "Unknown" : candidate.GenreName.Trim(),
            DurationSeconds = candidate.DurationSeconds > 0 ? candidate.DurationSeconds : 1,
            ReleaseDate = candidate.ReleaseDate ?? new DateTime(2000, 1, 1),
            Mood = "Energetic",
            IsExplicit = false
        };

        _logger.LogInformation("Selected MusicBrainz recording {RecordingId} for AI draft.", candidate.MusicBrainzId);
        return View("Index", new AiMusicImportViewModel
        {
            ApiConfigured = _ai.IsConfigured,
            GenreNames = GetGenreNames(),
            Prompt = $"Verified MusicBrainz result: {draft.Title} by {draft.ArtistName}",
            Draft = draft
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Confirm(AiSongDraft draft)
    {
        NormalizeSongDraft(draft);
        ModelState.Clear();
        TryValidateModel(draft);
        if (!ModelState.IsValid) return View("Index", WithDraft(draft, "Review the draft values."));

        var releaseDate = draft.ReleaseDate.Year < 1753 ? new DateTime(2000, 1, 1) : draft.ReleaseDate;

        var artistName = string.IsNullOrWhiteSpace(draft.ArtistName) ? "Unknown artist" : draft.ArtistName.Trim();
        var albumTitle = string.IsNullOrWhiteSpace(draft.AlbumTitle) ? "Unknown album" : draft.AlbumTitle.Trim();
        var artist = _context.Artists.FirstOrDefault(x => x.StageName.ToLower() == artistName.ToLower());
        if (artist == null)
        {
            artist = new Artist
            {
                ArtistId = _context.Artists.Any() ? _context.Artists.Max(x => x.ArtistId) + 1 : 1,
                StageName = artistName,
                Country = "Unknown",
                DebutDate = releaseDate,
                Biography = "Created automatically from an AI-assisted music import.",
                IsActive = true
            };
            _context.Artists.Add(artist);
        }

        var album = _context.Albums.FirstOrDefault(x =>
            x.Title.ToLower() == albumTitle.ToLower() && x.ArtistId == artist.ArtistId);
        if (album == null)
        {
            album = new Album
            {
                AlbumId = _context.Albums.Any() ? _context.Albums.Max(x => x.AlbumId) + 1 : 1,
                Title = albumTitle,
                ReleaseDate = releaseDate,
                Label = "Unknown",
                TotalTracks = 1,
                Rating = 0,
                Artist = artist
            };
            _context.Albums.Add(album);
        }

        var genre = _context.Genres.FirstOrDefault(x => x.Name.ToLower() == draft.GenreName.ToLower());
        genre ??= _context.Genres.First(x => x.Name == "Unknown");
        if (!Enum.TryParse<MoodType>(draft.Mood, true, out var mood)) return View("Index", WithDraft(draft, $"Mood '{draft.Mood}' is not supported."));

        if (_context.Songs.Any(x => x.Title.ToLower() == draft.Title.ToLower() && x.ArtistId == artist.ArtistId))
            return View("Index", WithDraft(draft, "That song already exists for this artist."));

        var song = new Song
        {
            SongId = _context.Songs.Any() ? _context.Songs.Max(x => x.SongId) + 1 : 1,
            Title = draft.Title.Trim(), Artist = artist, Album = album, Genre = genre,
            Duration = TimeSpan.FromSeconds(draft.DurationSeconds), ReleaseDate = releaseDate,
            PopularityScore = 0, PlayCount = 0, Mood = mood, IsExplicit = draft.IsExplicit
        };
        _context.Songs.Add(song);
        _context.SaveChanges();
        _logger.LogInformation("AI-assisted import created song {SongId}.", song.SongId);
        TempData["SuccessMessage"] = $"AI draft imported: {song.Title}";
        return RedirectToAction("Details", "Song", new { id = song.SongId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ConfirmArtist(AiArtistDraft draft)
    {
        NormalizeArtistDraft(draft);
        ModelState.Clear();
        TryValidateModel(draft);
        if (!ModelState.IsValid) return View("Index", WithArtistDraft(draft, "Review the artist draft values."));

        if (_context.Artists.Any(x => x.StageName.ToLower() == draft.StageName.ToLower()))
            return View("Index", WithArtistDraft(draft, "That artist already exists."));

        var artist = new Artist
        {
            ArtistId = _context.Artists.Any() ? _context.Artists.Max(x => x.ArtistId) + 1 : 1,
            StageName = draft.StageName,
            Country = draft.Country,
            DebutDate = SafeDate(draft.DebutDate),
            Biography = draft.Biography,
            IsActive = draft.IsActive
        };
        _context.Artists.Add(artist);
        _context.SaveChanges();
        _logger.LogInformation("AI-assisted import created artist {ArtistId}.", artist.ArtistId);
        TempData["SuccessMessage"] = $"AI draft imported: {artist.StageName}";
        return RedirectToAction("Details", "Artist", new { id = artist.ArtistId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ConfirmAlbum(AiAlbumDraft draft)
    {
        NormalizeAlbumDraft(draft);
        ModelState.Clear();
        TryValidateModel(draft);
        if (!ModelState.IsValid) return View("Index", WithAlbumDraft(draft, "Review the album draft values."));

        var artist = FindOrCreateArtist(draft.ArtistName, draft.ReleaseDate);
        if (_context.Albums.Any(x => x.Title.ToLower() == draft.Title.ToLower() && x.ArtistId == artist.ArtistId))
            return View("Index", WithAlbumDraft(draft, "That album already exists for this artist."));

        var album = new Album
        {
            AlbumId = _context.Albums.Any() ? _context.Albums.Max(x => x.AlbumId) + 1 : 1,
            Title = draft.Title,
            Artist = artist,
            ReleaseDate = SafeDate(draft.ReleaseDate),
            Label = draft.Label,
            TotalTracks = draft.TotalTracks,
            Rating = 0,
            CoverUrl = null
        };
        _context.Albums.Add(album);
        _context.SaveChanges();
        _logger.LogInformation("AI-assisted import created album {AlbumId} without requiring a cover.", album.AlbumId);
        TempData["SuccessMessage"] = $"AI draft imported: {album.Title}";
        return RedirectToAction("Details", "Album", new { id = album.AlbumId });
    }

    private AiMusicImportViewModel WithDraft(AiSongDraft draft, string error) => new()
    {
        ApiConfigured = _ai.IsConfigured, GenreNames = GetGenreNames(), Draft = draft, ErrorMessage = error, Prompt = "AI-generated draft"
    };

    private AiMusicImportViewModel WithArtistDraft(AiArtistDraft draft, string error) => new()
    {
        ApiConfigured = _ai.IsConfigured, GenreNames = GetGenreNames(), ArtistDraft = draft,
        ErrorMessage = error, Prompt = "AI-generated artist draft"
    };

    private AiMusicImportViewModel WithAlbumDraft(AiAlbumDraft draft, string error) => new()
    {
        ApiConfigured = _ai.IsConfigured, GenreNames = GetGenreNames(), AlbumDraft = draft,
        ErrorMessage = error, Prompt = "AI-generated album draft"
    };

    private AiMusicImportViewModel NewViewModel() => new()
    {
        ApiConfigured = _ai.IsConfigured,
        GenreNames = GetGenreNames()
    };

    private List<string> GetGenreNames() => _context.Genres
        .OrderBy(x => x.Name)
        .Select(x => x.Name)
        .ToList();

    private string ResolveGenreName(string? requested)
    {
        var match = _context.Genres
            .Select(x => x.Name)
            .AsEnumerable()
            .FirstOrDefault(name => string.Equals(name, requested?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? "Unknown";
    }

    private Artist FindOrCreateArtist(string name, DateTime date)
    {
        var safeName = KnownOr(name, "Unknown artist");
        var artist = _context.Artists.FirstOrDefault(x => x.StageName.ToLower() == safeName.ToLower());
        if (artist != null) return artist;

        artist = new Artist
        {
            ArtistId = _context.Artists.Any() ? _context.Artists.Max(x => x.ArtistId) + 1 : 1,
            StageName = safeName,
            Country = "Unknown",
            DebutDate = SafeDate(date),
            Biography = "Created automatically from an AI-assisted import.",
            IsActive = true
        };
        _context.Artists.Add(artist);
        return artist;
    }

    private static DateTime SafeDate(DateTime date) => date.Year < 1753 ? new DateTime(2000, 1, 1) : date;

    private static string KnownOr(params string?[] values) => values
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && !value.Equals("Unknown", StringComparison.OrdinalIgnoreCase))?.Trim()
        ?? values.LastOrDefault()?.Trim()
        ?? "Unknown";

    private static void NormalizeArtistDraft(AiArtistDraft draft)
    {
        draft.StageName = KnownOr(draft.StageName, "Unknown artist");
        draft.Country = KnownOr(draft.Country, "Unknown");
        draft.Biography = KnownOr(draft.Biography, "Unknown");
    }

    private string NormalizeMood(string? mood) => Enum.TryParse<MoodType>(mood, true, out var parsed)
        ? parsed.ToString()
        : MoodType.Energetic.ToString();

    private void NormalizeSongDraft(AiSongDraft draft)
    {
        draft.Title = KnownOr(draft.Title, "Unknown song");
        draft.ArtistName = KnownOr(draft.ArtistName, "Unknown artist");
        draft.AlbumTitle = KnownOr(draft.AlbumTitle, "Unknown album");
        draft.GenreName = ResolveGenreName(draft.GenreName);
        draft.DurationSeconds = Math.Clamp(draft.DurationSeconds, 1, 7200);
        draft.ReleaseDate = SafeDate(draft.ReleaseDate);
        draft.Mood = NormalizeMood(draft.Mood);
    }

    private static void NormalizeAlbumDraft(AiAlbumDraft draft)
    {
        draft.Title = KnownOr(draft.Title, "Unknown album");
        draft.ArtistName = KnownOr(draft.ArtistName, "Unknown artist");
        draft.Label = KnownOr(draft.Label, "Unknown");
        draft.TotalTracks = Math.Clamp(draft.TotalTracks, 0, 500);
    }
}
