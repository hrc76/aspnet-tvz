using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.MockRepositories;
using System.Diagnostics;

namespace Playlist.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SongMockRepository _songRepository;
        private readonly ArtistMockRepository _artistRepository;

        public HomeController(
            ILogger<HomeController> logger,
            SongMockRepository songRepository,
            ArtistMockRepository artistRepository)
        {
            _logger = logger;
            _songRepository = songRepository;
            _artistRepository = artistRepository;
        }

        public IActionResult Index()
        {
            var topSongs = _songRepository.GetAll()
                .OrderByDescending(s => s.PlayCount)
                .Take(5)
                .ToList();

            var featuredArtists = _artistRepository.GetAll()
                .Take(3)
                .ToList();

            ViewBag.TopSongs = topSongs;
            ViewBag.FeaturedArtists = featuredArtists;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}