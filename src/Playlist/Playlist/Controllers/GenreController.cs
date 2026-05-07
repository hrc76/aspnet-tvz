using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class GenreController : Controller
    {
        private readonly GenreRepository _genreRepository;

        public GenreController(GenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public IActionResult Index()
        {
            var genres = _genreRepository.GetAll();
            return View(genres);
        }

        public IActionResult Details(int id)
        {
            var genre = _genreRepository.GetById(id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }
    }
}