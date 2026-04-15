using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly PlaylistMockRepository _playlistRepository;

        public PlaylistController(PlaylistMockRepository playlistRepository)
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
    }
}