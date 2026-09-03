using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.ViewModels;

namespace Playlist.Services;

public sealed class AchievementService
{
    private readonly MusicBarDbContext _context;

    public AchievementService(MusicBarDbContext context) => _context = context;

    public async Task<List<AchievementViewModel>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var history = await _context.ListeningHistories.AsNoTracking()
            .Include(item => item.Song).ThenInclude(song => song.Artist)
            .Include(item => item.Song).ThenInclude(song => song.Genre)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.ListenedAt)
            .Take(20)
            .ToListAsync(cancellationToken);
        var plays = history.Sum(item => Math.Max(1, item.Repeats));
        var uniqueSongs = history.Select(item => item.SongId).Distinct().Count();
        var uniqueGenres = history.Select(item => item.Song.GenreId).Distinct().Count();
        var topArtistPlays = history.GroupBy(item => item.Song.ArtistId)
            .Select(group => group.Sum(item => Math.Max(1, item.Repeats))).DefaultIfEmpty(0).Max();
        var playlists = await _context.Playlists.AsNoTracking().CountAsync(item => item.OwnerId == userId, cancellationToken);
        var favorites = await _context.FavoriteSongs.AsNoTracking().CountAsync(item => item.UserId == userId, cancellationToken);
        var nightPlays = history.Where(item => item.ListenedAt.Hour < 5)
            .Sum(item => Math.Max(1, item.Repeats));

        return new List<AchievementViewModel>
        {
            Badge("FIRST.BEAT", "First Beat", "Complete your first qualified listen.", "▶", plays, 1),
            Badge("TRACK.HUNTER", "Track Hunter", "Listen to five different songs.", "♪", uniqueSongs, 5),
            Badge("LOOP.MODE", "Loop Mode", "Reach ten total plays in recent history.", "↺", plays, 10),
            Badge("GENRE.HOP", "Genre Hopper", "Explore three different genres.", "◆", uniqueGenres, 3),
            Badge("LOYAL.FAN", "Loyal Fan", "Play one artist at least five times.", "★", topArtistPlays, 5),
            Badge("CURATOR", "Playlist Curator", "Create three playlists.", "+", playlists, 3),
            Badge("COLLECTOR", "Track Collector", "Save five favorite songs.", "♥", favorites, 5),
            Badge("NIGHT.EXE", "Night Listener", "Record three plays between midnight and 5 AM.", "☾", nightPlays, 3),
            Badge("HISTORY.MAX", "Memory Full", "Fill all 20 recent-history slots.", "20", history.Count, 20)
        };
    }

    private static AchievementViewModel Badge(string code, string name, string description, string icon, int progress, int target) => new()
    {
        Code = code,
        Name = name,
        Description = description,
        Icon = icon,
        Progress = Math.Min(progress, target),
        Target = target
    };
}
