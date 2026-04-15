using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;

namespace Playlist.Controllers
{
    public class ListeningHistoryController : Controller
    {
        private readonly ListeningHistoryMockRepository _listeningHistoryRepository;

        public ListeningHistoryController(ListeningHistoryMockRepository listeningHistoryRepository)
        {
            _listeningHistoryRepository = listeningHistoryRepository;
        }

        public IActionResult Index()
        {
            var historyItems = _listeningHistoryRepository.GetAll();
            return View(historyItems);
        }

        public IActionResult Details(int id)
        {
            var historyItem = _listeningHistoryRepository.GetById(id);

            if (historyItem == null)
            {
                return NotFound();
            }

            return View(historyItem);
        }
    }
}