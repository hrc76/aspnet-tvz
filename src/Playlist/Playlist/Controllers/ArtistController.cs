using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class ArtistController : Controller
    {
        private readonly ArtistMockRepository _artistRepository;

        public ArtistController(ArtistMockRepository artistRepository)
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