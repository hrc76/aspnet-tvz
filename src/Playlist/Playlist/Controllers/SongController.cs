using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class SongController : Controller
    {
        private readonly SongMockRepository _songRepository;

        public SongController(SongMockRepository songRepository)
        {
            _songRepository = songRepository;
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

            return View(song);
        }
    }
}