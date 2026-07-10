using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.ViewModels;

namespace Playlist.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly SongRepository _songRepository;
        private readonly AlbumRepository _albumRepository;
        private readonly PlaylistRepository _playlistRepository;
        private readonly FavoriteSongRepository _favoriteSongRepository;
        private readonly SavedAlbumRepository _savedAlbumRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public LibraryController(
            SongRepository songRepository,
            AlbumRepository albumRepository,
            PlaylistRepository playlistRepository,
            FavoriteSongRepository favoriteSongRepository,
            SavedAlbumRepository savedAlbumRepository,
            UserManager<AppUser> userManager,
            UserRepository userRepository)
        {
            _songRepository = songRepository;
            _albumRepository = albumRepository;
            _playlistRepository = playlistRepository;
            _favoriteSongRepository = favoriteSongRepository;
            _savedAlbumRepository = savedAlbumRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentDomainUserIdAsync();

            var favoriteSongs = userId == null
                ? new List<Song>()
                : _favoriteSongRepository.GetFavoriteSongsByUserId(userId.Value);

            var savedAlbums = userId == null
                ? _albumRepository.GetAll()
                : _savedAlbumRepository.GetSavedAlbumsByUserId(userId.Value);

            var albumItems = savedAlbums
                .Select(a => new LibraryItemViewModel
                {
                    Id = a.AlbumId,
                    Type = "Album",
                    Title = a.Title,
                    Subtitle = a.Artist.StageName,
                    Meta = $"{a.ReleaseDate.Year} · {a.Rating} ★",
                    ImagePath = a.CoverUrl ?? string.Empty,
                    ControllerName = "Album"
                });

            var userPlaylists = userId == null
                ? _playlistRepository.GetAll()
                : _playlistRepository.GetByOwnerId(userId.Value);

            var playlistItems = userPlaylists
                .Select(p => new LibraryItemViewModel
                {
                    Id = p.PlaylistId,
                    Type = "Playlist",
                    Title = p.Name,
                    Subtitle = p.Owner.Username,
                    Meta = $"{p.Songs.Count} songs · {(p.IsPublic ? "Public" : "Private")}",
                    ImagePath = p.CoverImageUrl ?? string.Empty,
                    ControllerName = "Playlist"
                });

            var model = new LibraryViewModel
            {
                FavoriteSongs = favoriteSongs,
                LibraryItems = albumItems
                    .Concat(playlistItems)
                    .OrderBy(i => i.Title)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term, string type = "all")
        {
            term = term?.ToLower() ?? "";
            type = type?.ToLower() ?? "all";

            var userId = await GetCurrentDomainUserIdAsync();

            var savedAlbums = userId == null
                ? _albumRepository.GetAll()
                : _savedAlbumRepository.GetSavedAlbumsByUserId(userId.Value);

            var albumItems = savedAlbums
                .Select(a => new LibraryItemViewModel
                {
                    Id = a.AlbumId,
                    Type = "Album",
                    Title = a.Title,
                    Subtitle = a.Artist.StageName,
                    Meta = $"{a.ReleaseDate.Year} · {a.Rating} ★",
                    ImagePath = a.CoverUrl ?? string.Empty,
                    ControllerName = "Album"
                });

            var playlists = userId == null
                ? _playlistRepository.GetAll()
                : _playlistRepository.GetByOwnerId(userId.Value);

            var playlistItems = playlists
                .Select(p => new LibraryItemViewModel
                {
                    Id = p.PlaylistId,
                    Type = "Playlist",
                    Title = p.Name,
                    Subtitle = p.Owner.Username,
                    Meta = $"{p.Songs.Count} songs · {(p.IsPublic ? "Public" : "Private")}",
                    ImagePath = p.CoverImageUrl ?? string.Empty,
                    ControllerName = "Playlist"
                });

            var items = albumItems.Concat(playlistItems);

            if (type == "albums")
            {
                items = items.Where(i => i.Type == "Album");
            }
            else if (type == "playlists")
            {
                items = items.Where(i => i.Type == "Playlist");
            }

            if (!string.IsNullOrWhiteSpace(term))
            {
                items = items.Where(i =>
                    i.Title.ToLower().Contains(term) ||
                    i.Subtitle.ToLower().Contains(term) ||
                    i.Meta.ToLower().Contains(term));
            }

            var result = items
                .OrderBy(i => i.Title)
                .Select(i => new
                {
                    id = i.Id,
                    type = i.Type,
                    title = i.Title,
                    subtitle = i.Subtitle,
                    meta = i.Meta,
                    imagePath = i.ImagePath,
                    url = Url.Action("Details", i.ControllerName, new { id = i.Id })
                })
                .ToList();

            return Json(result);
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return null;
            }

            var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
            if (domainUser != null)
            {
                return domainUser.UserId;
            }

            domainUser = new User
            {
                Username = appUser.UserName ?? appUser.Email ?? "User",
                Email = appUser.Email ?? string.Empty,
                RegistrationDate = DateTime.UtcNow,
                IsPremium = false
            };
            _userRepository.Add(domainUser);
            return domainUser.UserId;
        }
    }
}
