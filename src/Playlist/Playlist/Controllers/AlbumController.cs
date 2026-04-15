using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumMockRepository _albumRepository;

        public AlbumController(AlbumMockRepository albumRepository)
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