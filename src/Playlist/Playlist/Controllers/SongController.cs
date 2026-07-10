using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class SongController : Controller
    {
        private static readonly HttpClient _deezerHttp = new() { Timeout = TimeSpan.FromSeconds(6) };
        private readonly SongRepository _songRepository;
        private readonly ArtistRepository _artistRepository;
        private readonly AlbumRepository _albumRepository;
        private readonly GenreRepository _genreRepository;
        private readonly PlaylistRepository _playlistRepository;
        private readonly FavoriteSongRepository _favoriteSongRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public SongController(
            SongRepository songRepository,
            ArtistRepository artistRepository,
            AlbumRepository albumRepository,
            GenreRepository genreRepository,
            PlaylistRepository playlistRepository,
            FavoriteSongRepository favoriteSongRepository,
            UserManager<AppUser> userManager,
            UserRepository userRepository)
        {
            _songRepository = songRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _genreRepository = genreRepository;
            _playlistRepository = playlistRepository;
            _favoriteSongRepository = favoriteSongRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var songs = _songRepository.GetAll();
            return View(songs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var song = _songRepository.GetById(id);
            if (song == null) return NotFound();

            ViewBag.Playlists = _playlistRepository.GetAll();
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(User);
                var domainUser = appUser == null
                    ? null
                    : _userRepository.GetByEmail(appUser.Email ?? string.Empty);
                userId = domainUser?.UserId;
            }

            ViewBag.IsFavorite = userId != null && _favoriteSongRepository.IsFavorite(userId.Value, song.SongId);
            return View(song);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var songs = _songRepository.Search(term);
            var result = songs.Select(s => new
            {
                id = s.SongId,
                title = s.Title,
                artist = s.Artist.StageName,
                genre = s.Genre.Name,
                duration = s.Duration.ToString(@"m\:ss")
            });
            return Json(result);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new Song { ReleaseDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Song song)
        {
            ModelState.Remove("Artist");
            ModelState.Remove("Album");
            ModelState.Remove("Genre");
            ModelState.Remove("Playlists");

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(song);
            }

            _songRepository.Add(song);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var song = _songRepository.GetById(id);
            if (song == null) return NotFound();
            LoadDropdowns();
            return View(song);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Song song)
        {
            if (id != song.SongId) return BadRequest();

            ModelState.Remove("Artist");
            ModelState.Remove("Album");
            ModelState.Remove("Genre");
            ModelState.Remove("Playlists");

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(song);
            }

            _songRepository.Update(song);
            return RedirectToAction(nameof(Details), new { id = song.SongId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _songRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeezerPreview(string artist, string title)
        {
            try
            {
                var q = Uri.EscapeDataString($"{artist} {title}".Trim());
                var json = await _deezerHttp.GetStringAsync($"https://api.deezer.com/search?q={q}&limit=5");
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
                {
                    var preview = data[0].GetProperty("preview").GetString();
                    if (!string.IsNullOrEmpty(preview))
                        return Json(new { url = preview });
                }
            }
            catch { }
            return Json(new { url = (string?)null });
        }

        private void LoadDropdowns()
        {
            ViewBag.Artists = _artistRepository.GetAll();
            ViewBag.Albums = _albumRepository.GetAll();
            ViewBag.Genres = _genreRepository.GetAll();
            ViewBag.Moods = Enum.GetValues(typeof(MoodType)).Cast<MoodType>().ToList();
        }
    }
}
