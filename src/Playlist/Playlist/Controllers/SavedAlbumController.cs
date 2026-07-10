using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    [Authorize]
    public class SavedAlbumController : Controller
    {
        private readonly SavedAlbumRepository _savedAlbumRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public SavedAlbumController(
            SavedAlbumRepository savedAlbumRepository,
            UserManager<AppUser> userManager,
            UserRepository userRepository)
        {
            _savedAlbumRepository = savedAlbumRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int albumId)
        {
            var userId = await GetCurrentDomainUserIdAsync();

            if (userId == null)
            {
                return Challenge();
            }

            _savedAlbumRepository.Add(userId.Value, albumId);

            return RedirectToAction("Details", "Album", new { id = albumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int albumId)
        {
            var userId = await GetCurrentDomainUserIdAsync();

            if (userId == null)
            {
                return Challenge();
            }

            _savedAlbumRepository.Remove(userId.Value, albumId);

            return RedirectToAction("Index", "Library");
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return null;

            var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
            if (domainUser != null) return domainUser.UserId;

            domainUser = new User
            {
                Username = appUser.UserName ?? appUser.Email ?? "User",
                Email = appUser.Email ?? string.Empty,
                RegistrationDate = DateTime.UtcNow,
                IsPremium = false
            };
            _userRepository.Add(domainUser);
            return domainUser.UserId;
        }
    }
}
