using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class DiscoverController : Controller
    {
        private readonly SongRepository _songRepository;
        private readonly ArtistRepository _artistRepository;
        private readonly AlbumRepository _albumRepository;
        private readonly GenreRepository _genreRepository;

        public DiscoverController(
            SongRepository songRepository,
            ArtistRepository artistRepository,
            AlbumRepository albumRepository,
            GenreRepository genreRepository)
        {
            _songRepository = songRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _genreRepository = genreRepository;
        }

        public IActionResult Index()
        {
            ViewBag.Songs = _songRepository.GetAll()
                .OrderByDescending(s => s.PopularityScore)
                .Take(7)
                .ToList();

            ViewBag.Artists = _artistRepository.GetAll()
                .Take(5)
                .ToList();

            ViewBag.Albums = _albumRepository.GetAll()
                .OrderByDescending(a => a.Rating)
                .Take(5)
                .ToList();

            ViewBag.Genres = _genreRepository.GetAll();

            return View();
        }

        [HttpGet]
public IActionResult Search(string term)
{
    term = term?.ToLower() ?? "";

    var songs = _songRepository.GetAll()
        .Where(s => s.Title.ToLower().Contains(term)
                 || s.Artist.StageName.ToLower().Contains(term)
                 || s.Album.Title.ToLower().Contains(term)
                 || s.Genre.Name.ToLower().Contains(term))
        .Take(10)
        .Select(s => new
        {
            id = s.SongId,
            title = s.Title,
            subtitle = $"{s.Artist.StageName} · {s.Album.Title} · {s.Genre.Name}",
            url = Url.Action("Details", "Song", new { id = s.SongId })
        })
        .ToList();

    return Json(songs);
}
    }
}