using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class SongController : Controller
    {
        private readonly SongRepository _songRepository;
        private readonly PlaylistRepository _playlistRepository;
        private readonly FavoriteSongRepository _favoriteSongRepository;

        public SongController(SongRepository songRepository, PlaylistRepository playlistRepository, FavoriteSongRepository favoriteSongRepository)
        {
            _songRepository = songRepository;
            _playlistRepository = playlistRepository;
            _favoriteSongRepository = favoriteSongRepository;  
        }

        public IActionResult Index()
        {
            var songs = _songRepository.GetAll();
            return View(songs);
        }

        public IActionResult Details(int id)
        {
            var song = _songRepository.GetById(id);

            if (song == null)
            {
                return NotFound();
            }

            ViewBag.Playlists = _playlistRepository.GetAll();
            var userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.IsFavorite = userId != null &&
            _favoriteSongRepository.IsFavorite(userId.Value, song.SongId);
            return View(song);
        }
    }
}