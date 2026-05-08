using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult SignIn()
        {
            return View(_userRepository.GetAll());
        }

        [HttpPost]
        public IActionResult SignIn(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Profile");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("SignIn");
            }

            var user = _userRepository.GetById(userId.Value);

            if (user == null)
            {
                return RedirectToAction("SignIn");
            }

            return View(user);
        }

        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}