using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumRepository _albumRepository;

        public AlbumController(AlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
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

            return View(album);
        }
    }
}