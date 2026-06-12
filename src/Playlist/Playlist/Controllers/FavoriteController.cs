using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly FavoriteSongRepository _favoriteSongRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public FavoriteController(
            FavoriteSongRepository favoriteSongRepository,
            UserManager<AppUser> userManager,
            UserRepository userRepository)
        {
            _favoriteSongRepository = favoriteSongRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int songId)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            _favoriteSongRepository.Add(userId.Value, songId);
            return RedirectToAction("Details", "Song", new { id = songId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int songId)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            _favoriteSongRepository.Remove(userId.Value, songId);
            return RedirectToAction("Details", "Song", new { id = songId });
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return null;
            }

            var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
            if (domainUser != null)
            {
                return domainUser.UserId;
            }

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
