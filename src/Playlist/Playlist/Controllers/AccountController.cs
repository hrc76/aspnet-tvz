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

            return RedirectToAction(nameof(Profile), new { id = user.UserId });
        }

        public IActionResult Profile(int? id)
        {
            if (id == null)
            {
                var sessionUserId = HttpContext.Session.GetInt32("UserId");

                if (sessionUserId == null)
                {
                    return RedirectToAction(nameof(SignIn));
                }

                id = sessionUserId.Value;
            }

            var user = _userRepository.GetById(id.Value);

            if (user == null)
            {
                return NotFound();
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