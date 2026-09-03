using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.ViewModels;

namespace Playlist.Controllers;

[Route("global-search")]
public sealed class GlobalSearchController : ControllerBase
{
    // Staticke stranice nemaju zapis u bazi, zato su navedene kao mali interni indeks.
    private static readonly GlobalSearchResult[] Pages =
    {
        Page("Page", "Home", "Application dashboard", "/"),
        Page("Page", "Discover", "Explore the music catalog", "/Discover"),
        Page("Page", "Library", "Saved songs, albums and playlists", "/Library"),
        Page("Page", "Songs", "Browse and manage songs", "/Song"),
        Page("Page", "Artists", "Browse and manage artists", "/Artist"),
        Page("Page", "Albums", "Browse and manage albums", "/Album"),
        Page("Page", "Genres", "Browse and manage genres", "/Genre"),
        Page("Page", "Playlists", "Browse playlists", "/Playlist"),
        Page("Page", "Users", "Browse application users", "/User"),
        Page("Page", "Listening history", "Recently played music", "/ListeningHistory")
    };

    private readonly MusicBarDbContext _context;

    public GlobalSearchController(MusicBarDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GlobalSearchResult>>> Get([FromQuery] string? term)
    {
        term = term?.Trim();
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return Ok(Array.Empty<GlobalSearchResult>());
        }

        // Svaka kategorija daje najvise cetiri rezultata, a konacni odgovor najvise 20.
        // AsNoTracking ubrzava upite jer rezultate pretrage nikada ne mijenjamo.
        var pageResults = Pages
            .Where(page => page.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || page.Subtitle.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(4);

        var songs = await _context.Songs
            .AsNoTracking()
            .Where(song => song.Title.Contains(term) || song.Artist.StageName.Contains(term))
            .OrderBy(song => song.Title)
            .Take(4)
            .Select(song => new GlobalSearchResult
            {
                Type = "Song",
                Title = song.Title,
                Subtitle = song.Artist.StageName,
                Url = "/Song/Details/" + song.SongId
            })
            .ToListAsync();

        var artists = await _context.Artists
            .AsNoTracking()
            .Where(artist => artist.StageName.Contains(term) || artist.Country.Contains(term))
            .OrderBy(artist => artist.StageName)
            .Take(4)
            .Select(artist => new GlobalSearchResult
            {
                Type = "Artist",
                Title = artist.StageName,
                Subtitle = artist.Country,
                Url = "/Artist/Details/" + artist.ArtistId
            })
            .ToListAsync();

        var albums = await _context.Albums
            .AsNoTracking()
            .Where(album => album.Title.Contains(term) || album.Artist.StageName.Contains(term))
            .OrderBy(album => album.Title)
            .Take(4)
            .Select(album => new GlobalSearchResult
            {
                Type = "Album",
                Title = album.Title,
                Subtitle = album.Artist.StageName,
                Url = "/Album/Details/" + album.AlbumId
            })
            .ToListAsync();

        var genres = await _context.Genres
            .AsNoTracking()
            .Where(genre => genre.Name.Contains(term) || genre.Description.Contains(term))
            .OrderBy(genre => genre.Name)
            .Take(4)
            .Select(genre => new GlobalSearchResult
            {
                Type = "Genre",
                Title = genre.Name,
                Subtitle = "Music genre",
                Url = "/Genre/Details/" + genre.GenreId
            })
            .ToListAsync();

        var playlists = await _context.Playlists
            .AsNoTracking()
            .Where(playlist => playlist.IsPublic
                && (playlist.Name.Contains(term) || playlist.Description.Contains(term)))
            .OrderBy(playlist => playlist.Name)
            .Take(4)
            .Select(playlist => new GlobalSearchResult
            {
                Type = "Playlist",
                Title = playlist.Name,
                Subtitle = "Public playlist",
                Url = "/Playlist/Details/" + playlist.PlaylistId
            })
            .ToListAsync();

        var users = await _context.Users
            .AsNoTracking()
            .Where(user => user.Username.Contains(term))
            .OrderBy(user => user.Username)
            .Take(4)
            .Select(user => new GlobalSearchResult
            {
                Type = "User",
                Title = user.Username,
                Subtitle = user.IsPremium ? "Premium user" : "User",
                Url = "/User/Details/" + user.UserId
            })
            .ToListAsync();

        return Ok(pageResults
            .Concat(songs)
            .Concat(artists)
            .Concat(albums)
            .Concat(genres)
            .Concat(playlists)
            .Concat(users)
            .Take(20));
    }

    private static GlobalSearchResult Page(string type, string title, string subtitle, string url) =>
        new() { Type = type, Title = title, Subtitle = subtitle, Url = url };
}
