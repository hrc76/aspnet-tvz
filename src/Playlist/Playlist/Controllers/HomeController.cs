using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;
using System.Diagnostics;

namespace Playlist.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SongRepository _songRepository;
        private readonly ArtistRepository _artistRepository;
        private readonly AlbumRepository _albumRepository;
        private readonly GenreRepository _genreRepository;

        public HomeController(
            ILogger<HomeController> logger,
            SongRepository songRepository,
            ArtistRepository artistRepository,
            AlbumRepository albumRepository,
            GenreRepository genreRepository)
        {
            _logger = logger;
            _songRepository = songRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _genreRepository = genreRepository;
        }

        public IActionResult Index()
        {
            var topSongs = _songRepository.GetAll()
                .OrderByDescending(s => s.PlayCount)
                .Take(5)
                .ToList();

            var featuredAlbums = _albumRepository.GetAll()
                .OrderByDescending(a => a.Rating)
                .Take(4)
                .ToList();

            var featuredGenres = _genreRepository.GetAll()
                .Take(5)
                .ToList();

            var featuredArtists = _artistRepository.GetAll()
                .Take(3)
                .ToList();

            var heroAlbum = featuredAlbums.FirstOrDefault();

            ViewBag.TopSongs = topSongs;
            ViewBag.FeaturedAlbums = featuredAlbums;
            ViewBag.FeaturedGenres = featuredGenres;
            ViewBag.FeaturedArtists = featuredArtists;
            ViewBag.HeroAlbum = heroAlbum;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}