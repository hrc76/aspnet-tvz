using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class ArtistController : Controller
    {
        private readonly ArtistRepository _artistRepository;

        public ArtistController(ArtistRepository artistRepository)
        {
            _artistRepository = artistRepository;
        }

        public IActionResult Index()
        {
            var artists = _artistRepository.GetAll();
            return View(artists);
        }

        public IActionResult Details(int id)
        {
            var artist = _artistRepository.GetById(id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var artists = _artistRepository.Search(term);
            var result = artists.Select(a => new
            {
                id = a.ArtistId,
                name = a.StageName,
                country = a.Country,
                active = a.IsActive ? "Active" : "Inactive"
            });
            return Json(result);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new Artist { DebutDate = DateTime.Today, IsActive = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Artist artist)
        {
            ModelState.Remove("Albums");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
                return View(artist);

            _artistRepository.Add(artist);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var artist = _artistRepository.GetById(id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Artist artist)
        {
            if (id != artist.ArtistId) return BadRequest();

            ModelState.Remove("Albums");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
                return View(artist);

            _artistRepository.Update(artist);
            return RedirectToAction(nameof(Details), new { id = artist.ArtistId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _artistRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
