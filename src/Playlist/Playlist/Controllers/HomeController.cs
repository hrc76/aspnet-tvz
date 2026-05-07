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

        public HomeController(
            ILogger<HomeController> logger,
            SongRepository songRepository,
            ArtistRepository artistRepository)
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
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}