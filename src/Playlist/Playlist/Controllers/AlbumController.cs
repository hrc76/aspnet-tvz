using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.Services;

namespace Playlist.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumRepository _albumRepository;
        private readonly ArtistRepository _artistRepository;
        private readonly SavedAlbumRepository _savedAlbumRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;
        private readonly IImageStorageService _imageStorage;

        public AlbumController(AlbumRepository albumRepository, ArtistRepository artistRepository,
            SavedAlbumRepository savedAlbumRepository, UserManager<AppUser> userManager,
            UserRepository userRepository, IImageStorageService imageStorage)
        {
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _savedAlbumRepository = savedAlbumRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _imageStorage = imageStorage;
        }

        public IActionResult Index()
        {
            var albums = _albumRepository.GetAll();
            return View(albums);
        }

        public async Task<IActionResult> Details(int id)
        {
            var album = _albumRepository.GetById(id);
            if (album == null) return NotFound();

            var userId = await GetCurrentDomainUserIdAsync();
            ViewBag.IsSaved = userId != null && _savedAlbumRepository.IsSaved(userId.Value, album.AlbumId);
            return View(album);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var albums = _albumRepository.Search(term);
            var result = albums.Select(a => new
            {
                id = a.AlbumId,
                title = a.Title,
                artist = a.Artist.StageName,
                year = a.ReleaseDate.Year,
                rating = a.Rating
            });
            return Json(result);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.Artists = _artistRepository.GetAll();
            return View(new Album { ReleaseDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Album album, IFormFile? coverImage)
        {
            ModelState.Remove("Artist");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                ViewBag.Artists = _artistRepository.GetAll();
                return View(album);
            }

            album.CoverUrl = null;

            if (coverImage is { Length: > 0 })
            {
                try
                {
                    album.CoverUrl = await _imageStorage.SaveAsync(coverImage, "album-covers");
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("coverImage", exception.Message);
                    ViewBag.Artists = _artistRepository.GetAll();
                    return View(album);
                }
            }

            _albumRepository.Add(album);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var album = _albumRepository.GetById(id);
            if (album == null) return NotFound();
            ViewBag.Artists = _artistRepository.GetAll();
            return View(album);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Album album, IFormFile? coverImage)
        {
            if (id != album.AlbumId) return BadRequest();

            var existing = _albumRepository.GetById(id);
            if (existing == null) return NotFound();

            ModelState.Remove("Artist");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                album.CoverUrl = existing.CoverUrl;
                ViewBag.Artists = _artistRepository.GetAll();
                return View(album);
            }

            album.CoverUrl = existing.CoverUrl;
            if (coverImage is { Length: > 0 })
            {
                try
                {
                    album.CoverUrl = await _imageStorage.SaveAsync(
                        coverImage,
                        "album-covers",
                        existing.CoverUrl);
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("coverImage", exception.Message);
                    ViewBag.Artists = _artistRepository.GetAll();
                    return View(album);
                }
            }

            _albumRepository.Update(album);
            return RedirectToAction(nameof(Details), new { id = album.AlbumId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var album = _albumRepository.GetById(id);
            if (album == null) return NotFound();
            _albumRepository.Delete(id);
            _imageStorage.Delete(album.CoverUrl, "album-covers");
            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated != true) return null;
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return null;
            return _userRepository.GetByEmail(appUser.Email ?? string.Empty)?.UserId;
        }
    }
}
