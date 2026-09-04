using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.Services;
using System.IO;

namespace Playlist.Controllers
{
    [Authorize]
    public class PlaylistController : Controller
    {
        private readonly PlaylistRepository _playlistRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;
        private readonly IImageStorageService _imageStorage;

        public PlaylistController(
            PlaylistRepository playlistRepository,
            UserManager<AppUser> userManager,
            UserRepository userRepository,
            IImageStorageService imageStorage)
        {
            _playlistRepository = playlistRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _imageStorage = imageStorage;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentDomainUserIdAsync();
            var playlists = _playlistRepository.GetAll()
                .Where(p => p.IsPublic || IsCatalogAdmin() || (userId != null && p.OwnerId == userId.Value))
                .ToList();
            return View(playlists);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var playlist = _playlistRepository.GetById(id);

            if (playlist == null)
            {
                return NotFound();
            }

            var currentUserId = await GetCurrentDomainUserIdAsync();
            ViewBag.CanManage = IsCatalogAdmin() || (currentUserId != null && playlist.OwnerId == currentUserId.Value);

            if (!playlist.IsPublic && !IsCatalogAdmin())
            {
                if (currentUserId == null || playlist.OwnerId != currentUserId.Value) return Forbid();
            }

            return View(playlist);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Playlist.Models.Playlist playlist, IFormFile? coverImage)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Owner");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                return View(playlist);
            }

            playlist.OwnerId = userId.Value;
            playlist.CreatedAt = DateTime.Now;
            playlist.Likes = 0;
            playlist.Songs = new List<Song>();
            playlist.CoverImageUrl = null;

            if (coverImage is { Length: > 0 })
            {
                try
                {
                    playlist.CoverImageUrl = await _imageStorage.SaveAsync(coverImage, "playlist-covers");
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("coverImage", exception.Message);
                    return View(playlist);
                }
            }

            _playlistRepository.Add(playlist);

            return RedirectToAction("Index", "Library");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            var playlist = _playlistRepository.GetById(playlistId);
            if (playlist == null || (!IsCatalogAdmin() && (userId == null || playlist.OwnerId != userId.Value)))
            {
                return Forbid();
            }

            _playlistRepository.AddSongToPlaylist(playlistId, songId);
            return RedirectToAction("Details", "Song", new { id = songId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            var playlist = _playlistRepository.GetById(playlistId);
            var userId = await GetCurrentDomainUserIdAsync();
            if (playlist == null) return NotFound();
            if (!IsCatalogAdmin() && (userId == null || playlist.OwnerId != userId.Value)) return Forbid();
            _playlistRepository.RemoveSongFromPlaylist(playlistId, songId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var playlist = _playlistRepository.GetById(id);
            var userId = await GetCurrentDomainUserIdAsync();
            if (playlist == null) return NotFound();
            if (!IsCatalogAdmin() && (userId == null || playlist.OwnerId != userId.Value)) return Forbid();
            _playlistRepository.Delete(id);
            _imageStorage.Delete(playlist.CoverImageUrl, "playlist-covers");
            return RedirectToAction("Index", "Library");
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            term = term?.ToLower() ?? "";
            var userId = await GetCurrentDomainUserIdAsync();
            var result = _playlistRepository.GetAll()
                .Where(p => p.IsPublic || IsCatalogAdmin() || (userId != null && p.OwnerId == userId.Value))
                .Where(p => p.Name.ToLower().Contains(term) || (p.Owner != null && p.Owner.Username.ToLower().Contains(term)))
                .Take(12)
                .Select(p => new {
                    id       = p.PlaylistId,
                    text     = p.Name,
                    subtitle = $"by {p.Owner.Username} · {p.Songs.Count} songs"
                })
                .ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUserPlaylists(string term)
        {
            term = term?.ToLower() ?? "";
            var userId = await GetCurrentDomainUserIdAsync();

            var playlists = userId == null
                ? new List<Playlist.Models.Playlist>()
                : _playlistRepository.GetByOwnerId(userId.Value);

            var result = playlists
                .Where(p => p.Name.ToLower().Contains(term))
                .Take(10)
                .Select(p => new
                {
                    id = p.PlaylistId,
                    text = p.Name,
                    subtitle = $"{p.Songs.Count} songs · {(p.IsPublic ? "Public" : "Private")}"
                })
                .ToList();

            return Json(result);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var playlist = _playlistRepository.GetById(id);

            if (playlist == null)
            {
                return NotFound();
            }

            var userId = await GetCurrentDomainUserIdAsync();
            if (!IsCatalogAdmin() && (userId == null || playlist.OwnerId != userId.Value)) return Forbid();

            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Playlist.Models.Playlist playlist, IFormFile? coverImage, bool removeCoverImage = false)
        {
            if (id != playlist.PlaylistId)
            {
                return NotFound();
            }

            var existing = _playlistRepository.GetById(id);
            var userId = await GetCurrentDomainUserIdAsync();
            if (existing == null) return NotFound();
            if (!IsCatalogAdmin() && (userId == null || existing.OwnerId != userId.Value)) return Forbid();

            ModelState.Remove("Owner");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                playlist.CoverImageUrl = existing.CoverImageUrl;
                return View(playlist);
            }

            var coverImageUrl = existing.CoverImageUrl;
            if (coverImage is { Length: > 0 })
            {
                try
                {
                    coverImageUrl = await _imageStorage.SaveAsync(
                        coverImage,
                        "playlist-covers",
                        existing.CoverImageUrl);
                }
                catch (InvalidOperationException exception)
                {
                    playlist.CoverImageUrl = existing.CoverImageUrl;
                    ModelState.AddModelError("coverImage", exception.Message);
                    return View(playlist);
                }
            }
            else if (removeCoverImage)
            {
                _imageStorage.Delete(existing.CoverImageUrl, "playlist-covers");
                coverImageUrl = null;
            }

            _playlistRepository.UpdateBasicInfo(
                playlist.PlaylistId,
                playlist.Name,
                playlist.Description,
                playlist.IsPublic,
                coverImageUrl
            );

            return RedirectToAction(nameof(Details), new { id = playlist.PlaylistId });
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(User);
                if (appUser != null)
                {
                    var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
                    if (domainUser != null)
                    {
                        return domainUser.UserId;
                    }

                    var newDomainUser = new User
                    {
                        Username = appUser.UserName ?? appUser.Email ?? "Guest",
                        Email = appUser.Email ?? string.Empty,
                        RegistrationDate = DateTime.UtcNow,
                        IsPremium = false
                    };
                    _userRepository.Add(newDomainUser);
                    return newDomainUser.UserId;
                }
            }

            return HttpContext.Session.GetInt32("UserId");
        }

        private bool IsCatalogAdmin() => User.IsInRole("Admin") || User.IsInRole("Manager");
    }
}
