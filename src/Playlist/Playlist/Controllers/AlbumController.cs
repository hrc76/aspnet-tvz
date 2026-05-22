using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumRepository _albumRepository;

        private readonly SavedAlbumRepository _savedAlbumRepository;

        public AlbumController(AlbumRepository albumRepository, SavedAlbumRepository savedAlbumRepository)
        {
            _albumRepository = albumRepository;
            _savedAlbumRepository = savedAlbumRepository;
        }

        public IActionResult Index()
        {
            var albums = _albumRepository.GetAll();
            return View(albums);
        }

        public IActionResult Details(int id)
        {
            var album = _albumRepository.GetById(id);

            if (album == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetInt32("UserId");

                ViewBag.IsSaved = userId != null &&
                _savedAlbumRepository.IsSaved(userId.Value, album.AlbumId);
            return View(album);
        }
    }
}