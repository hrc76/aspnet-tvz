using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class UserController : Controller
    {
        private readonly UserMockRepository _userRepository;

        public UserController(UserMockRepository userRepository)
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

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}