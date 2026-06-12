using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
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
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var genres = _genreRepository.Search(term);
            var result = genres.Select(g => new
            {
                id = g.GenreId,
                name = g.Name,
                description = g.Description.Length > 60 ? g.Description.Substring(0, 60) + "..." : g.Description
            });
            return Json(result);
        }

        public IActionResult Create()
        {
            return View(new Genre());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Genre genre)
        {
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
                return View(genre);

            _genreRepository.Add(genre);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var genre = _genreRepository.GetById(id);
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Genre genre)
        {
            if (id != genre.GenreId) return BadRequest();

            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
                return View(genre);

            _genreRepository.Update(genre);
            return RedirectToAction(nameof(Details), new { id = genre.GenreId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _genreRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
