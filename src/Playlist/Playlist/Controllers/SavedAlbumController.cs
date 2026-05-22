using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class SavedAlbumController : Controller
    {
        private readonly SavedAlbumRepository _savedAlbumRepository;

        public SavedAlbumController(SavedAlbumRepository savedAlbumRepository)
        {
            _savedAlbumRepository = savedAlbumRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int albumId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            _savedAlbumRepository.Add(userId.Value, albumId);

            return RedirectToAction("Details", "Album", new { id = albumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int albumId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            _savedAlbumRepository.Remove(userId.Value, albumId);

            return RedirectToAction("Index", "Library");
        }
    }
}