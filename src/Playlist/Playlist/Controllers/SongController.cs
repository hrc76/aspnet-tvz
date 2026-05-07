using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class SongController : Controller
    {
        private readonly SongRepository _songRepository;

        public SongController(SongRepository songRepository)
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