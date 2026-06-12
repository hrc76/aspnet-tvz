using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class AlbumController : Controller
    {
        private readonly AlbumRepository _albumRepository;
        private readonly ArtistRepository _artistRepository;
        private readonly SavedAlbumRepository _savedAlbumRepository;

        public AlbumController(AlbumRepository albumRepository, ArtistRepository artistRepository, SavedAlbumRepository savedAlbumRepository)
        {
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
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
            if (album == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.IsSaved = userId != null && _savedAlbumRepository.IsSaved(userId.Value, album.AlbumId);
            return View(album);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var albums = _albumRepository.Search(term);
            var result = albums.Select(a => new
            {
                id = a.AlbumId,
                title = a.Title,
                artist = a.Artist.StageName,
                year = a.ReleaseDate.Year,
                rating = a.Rating
            });
            return Json(result);
        }

        public IActionResult Create()
        {
            ViewBag.Artists = _artistRepository.GetAll();
            return View(new Album { ReleaseDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Album album)
        {
            ModelState.Remove("Artist");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                ViewBag.Artists = _artistRepository.GetAll();
                return View(album);
            }

            _albumRepository.Add(album);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var album = _albumRepository.GetById(id);
            if (album == null) return NotFound();
            ViewBag.Artists = _artistRepository.GetAll();
            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Album album)
        {
            if (id != album.AlbumId) return BadRequest();

            ModelState.Remove("Artist");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                ViewBag.Artists = _artistRepository.GetAll();
                return View(album);
            }

            _albumRepository.Update(album);
            return RedirectToAction(nameof(Details), new { id = album.AlbumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _albumRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
