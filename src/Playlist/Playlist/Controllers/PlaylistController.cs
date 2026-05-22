using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly PlaylistRepository _playlistRepository;

        public PlaylistController(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public IActionResult Index()
        {
            var playlists = _playlistRepository.GetAll();
            return View(playlists);
        }

        public IActionResult Details(int id)
        {
            var playlist = _playlistRepository.GetById(id);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Playlist.Models.Playlist playlist)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
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

            _playlistRepository.Add(playlist);

            return RedirectToAction("Index", "Library");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSong(int playlistId, int songId)
        {
            _playlistRepository.AddSongToPlaylist(playlistId, songId);
            return RedirectToAction("Details", new { id = playlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSong(int playlistId, int songId)
        {
            _playlistRepository.RemoveSongFromPlaylist(playlistId, songId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _playlistRepository.Delete(id);
            return RedirectToAction("Index", "Library");
        }

        [HttpGet]
        public IActionResult SearchUserPlaylists(string term)
        {
            term = term?.ToLower() ?? "";

            var userId = HttpContext.Session.GetInt32("UserId");

            var playlists = userId == null
                ? _playlistRepository.GetAll()
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

        public IActionResult Edit(int id)
{
    var playlist = _playlistRepository.GetById(id);

    if (playlist == null)
    {
        return NotFound();
    }

    return View(playlist);
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(int id, Playlist.Models.Playlist playlist)
{
    if (id != playlist.PlaylistId)
    {
        return NotFound();
    }

    ModelState.Remove("Owner");
    ModelState.Remove("Songs");

    if (!ModelState.IsValid)
    {
        return View(playlist);
    }

    _playlistRepository.UpdateBasicInfo(
        playlist.PlaylistId,
        playlist.Name,
        playlist.Description,
        playlist.IsPublic
    );

    return RedirectToAction(nameof(Details), new { id = playlist.PlaylistId });
}
    }
}