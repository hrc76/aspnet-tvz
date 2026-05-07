using Microsoft.AspNetCore.Mvc;
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

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }
    }
}