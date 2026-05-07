using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class DiscoverController : Controller
    {
        private readonly SongMockRepository _songRepository;
        private readonly ArtistMockRepository _artistRepository;
        private readonly AlbumMockRepository _albumRepository;
        private readonly GenreMockRepository _genreRepository;

        public DiscoverController(
            SongMockRepository songRepository,
            ArtistMockRepository artistRepository,
            AlbumMockRepository albumRepository,
            GenreMockRepository genreRepository)
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
                .Take(6)
                .ToList();

            ViewBag.Artists = _artistRepository.GetAll()
                .Take(6)
                .ToList();

            ViewBag.Albums = _albumRepository.GetAll()
                .OrderByDescending(a => a.Rating)
                .Take(6)
                .ToList();

            ViewBag.Genres = _genreRepository.GetAll();

            return View();
        }
    }
}