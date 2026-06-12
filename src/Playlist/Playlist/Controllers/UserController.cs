using Microsoft.AspNetCore.Mvc;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var users = _userRepository.Search(term);
            var result = users.Select(u => new
            {
                id = u.UserId,
                username = u.Username,
                email = u.Email,
                premium = u.IsPremium ? "Premium" : "Free"
            });
            return Json(result);
        }

        public IActionResult Create()
        {
            return View(new User { RegistrationDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            ModelState.Remove("Playlists");
            ModelState.Remove("ListeningHistory");
            ModelState.Remove("FavoriteSongs");
            ModelState.Remove("SavedAlbums");

            if (!ModelState.IsValid)
                return View(user);

            _userRepository.Add(user);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User user)
        {
            if (id != user.UserId) return BadRequest();

            ModelState.Remove("Playlists");
            ModelState.Remove("ListeningHistory");
            ModelState.Remove("FavoriteSongs");
            ModelState.Remove("SavedAlbums");

            if (!ModelState.IsValid)
                return View(user);

            _userRepository.Update(user);
            return RedirectToAction(nameof(Details), new { id = user.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _userRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
