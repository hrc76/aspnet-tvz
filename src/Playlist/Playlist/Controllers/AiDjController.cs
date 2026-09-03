using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.Services;
using Playlist.ViewModels;

namespace Playlist.Controllers;

[Authorize]
public sealed class AiDjController : Controller
{
    private readonly IAiDjService _aiDj;
    private readonly MusicBarDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly PlaylistRepository _playlistRepository;
    private readonly ILogger<AiDjController> _logger;

    public AiDjController(IAiDjService aiDj, MusicBarDbContext context, UserManager<AppUser> userManager,
        UserRepository userRepository, PlaylistRepository playlistRepository, ILogger<AiDjController> logger)
    {
        _aiDj = aiDj;
        _context = context;
        _userManager = userManager;
        _userRepository = userRepository;
        _playlistRepository = playlistRepository;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View(new AiDjViewModel { ApiConfigured = _aiDj.IsConfigured });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(AiDjViewModel model, CancellationToken cancellationToken)
    {
        model.ApiConfigured = _aiDj.IsConfigured;
        if (!ModelState.IsValid) return View("Index", model);

        try
        {
            // AI ne pretrazuje internet: dobiva samo MusicBar katalog i korisnikov profil.
            // Zato rezultat uvijek mozemo povezati sa stvarnim Song zapisima iz baze.
            var catalogSongs = await LoadCatalogAsync(cancellationToken);
            var domainUser = await GetCurrentDomainUserAsync();
            var profile = await BuildProfileAsync(domainUser?.UserId, cancellationToken);
            model.Recommendation = await _aiDj.RecommendAsync(
                model.Request.Trim(),
                catalogSongs.Select(ToCatalogSong).ToList(),
                profile,
                cancellationToken);
            var positions = model.Recommendation.SongIds.Select((id, index) => new { id, index })
                .ToDictionary(item => item.id, item => item.index);
            model.Songs = catalogSongs.Where(song => positions.ContainsKey(song.SongId))
                .OrderBy(song => positions[song.SongId]).ToList();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(499);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "AI DJ recommendation failed.");
            model.ErrorMessage = exception.Message;
        }

        return View("Index", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AiDjSaveRequest request, CancellationToken cancellationToken)
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null) return Challenge();

        // Ne vjerujemo skrivenim poljima iz browsera: uklanjamo duplikate,
        // ogranicavamo velicinu i zatim ponovno trazimo pjesme u bazi.
        request.SongIds = request.SongIds.Distinct().Take(12).ToList();
        if (request.SongIds.Count == 0) ModelState.AddModelError("SongIds", "The AI mix has no valid songs.");
        if (!ModelState.IsValid)
        {
            var songs = await _context.Songs.Include(song => song.Artist).Include(song => song.Album)
                .Include(song => song.Genre).Where(song => request.SongIds.Contains(song.SongId))
                .ToListAsync(cancellationToken);
            return View("Index", new AiDjViewModel
            {
                ApiConfigured = _aiDj.IsConfigured,
                Request = "Saved AI DJ recommendation",
                ErrorMessage = "Review the playlist name and description.",
                Recommendation = new AiDjRecommendation
                {
                    PlaylistName = request.PlaylistName,
                    Description = request.Description,
                    SongIds = request.SongIds
                },
                Songs = songs
            });
        }

        var selectedSongs = await _context.Songs.Where(song => request.SongIds.Contains(song.SongId)).ToListAsync(cancellationToken);
        if (selectedSongs.Count == 0) return BadRequest();
        var playlist = new global::Playlist.Models.Playlist
        {
            Name = request.PlaylistName.Trim(),
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsPublic = request.IsPublic,
            Likes = 0,
            OwnerId = domainUser.UserId,
            Songs = selectedSongs
        };
        _playlistRepository.Add(playlist);
        _logger.LogInformation("User {UserId} saved AI DJ playlist {PlaylistId} with {SongCount} songs.",
            domainUser.UserId, playlist.PlaylistId, selectedSongs.Count);
        TempData["SuccessMessage"] = $"AI DJ mix saved: {playlist.Name}";
        return RedirectToAction("Details", "Playlist", new { id = playlist.PlaylistId });
    }

    private async Task<List<Song>> LoadCatalogAsync(CancellationToken cancellationToken) => await _context.Songs
        .AsNoTracking().Include(song => song.Artist).Include(song => song.Album).Include(song => song.Genre)
        .OrderByDescending(song => song.PopularityScore).Take(100).ToListAsync(cancellationToken);

    private async Task<User?> GetCurrentDomainUserAsync()
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser?.Email == null) return null;
        var domainUser = _userRepository.GetByEmail(appUser.Email);
        if (domainUser != null) return domainUser;
        domainUser = new User
        {
            Username = appUser.UserName ?? appUser.Email,
            Email = appUser.Email,
            RegistrationDate = DateTime.UtcNow,
            FavoriteGenreName = "Not selected"
        };
        _userRepository.Add(domainUser);
        return domainUser;
    }

    private async Task<AiDjListenerProfile> BuildProfileAsync(int? userId, CancellationToken cancellationToken)
    {
        // Personalizacija je "soft" signal: favoriti, albumi i history pomazu AI-ju,
        // ali korisnikov trenutni tekstualni zahtjev ima prednost.
        if (userId == null) return new AiDjListenerProfile();
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        return new AiDjListenerProfile
        {
            FavoriteGenre = string.IsNullOrWhiteSpace(user?.FavoriteGenreName) ? "Not selected" : user.FavoriteGenreName,
            FavoriteSongIds = await _context.FavoriteSongs.AsNoTracking().Where(item => item.UserId == userId)
                .Select(item => item.SongId).ToListAsync(cancellationToken),
            SavedAlbumIds = await _context.SavedAlbums.AsNoTracking().Where(item => item.UserId == userId)
                .Select(item => item.AlbumId).ToListAsync(cancellationToken),
            ListeningCounts = await _context.ListeningHistories.AsNoTracking().Where(item => item.UserId == userId)
                .GroupBy(item => item.SongId).ToDictionaryAsync(group => group.Key, group => group.Sum(item => item.Repeats), cancellationToken)
        };
    }

    private static AiDjCatalogSong ToCatalogSong(Song song) => new()
    {
        SongId = song.SongId,
        Title = song.Title,
        Artist = song.Artist.StageName,
        Album = song.Album.Title,
        Genre = song.Genre.Name,
        Mood = song.Mood.ToString(),
        DurationSeconds = (int)song.Duration.TotalSeconds,
        IsExplicit = song.IsExplicit,
        Popularity = song.PopularityScore
    };
}
