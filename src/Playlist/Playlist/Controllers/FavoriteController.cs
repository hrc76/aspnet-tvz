using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly FavoriteSongRepository _favoriteSongRepository;

        public FavoriteController(FavoriteSongRepository favoriteSongRepository)
        {
            _favoriteSongRepository = favoriteSongRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int songId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            _favoriteSongRepository.Add(userId.Value, songId);

            return RedirectToAction("Details", "Song", new { id = songId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int songId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            _favoriteSongRepository.Remove(userId.Value, songId);

            return RedirectToAction("Index", "Library");
        }
    }
}