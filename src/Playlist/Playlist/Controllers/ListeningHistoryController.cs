using Microsoft.AspNetCore.Mvc;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    public class ListeningHistoryController : Controller
    {
        private readonly ListeningHistoryRepository _listeningHistoryRepository;

        public ListeningHistoryController(ListeningHistoryRepository listeningHistoryRepository)
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

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var results = _listeningHistoryRepository.Search(term);
            return Json(results.Select(h => new
            {
                id = h.ListeningHistoryId,
                text = h.Song.Title,
                subtitle = $"{h.User.Username} · {h.Song.Artist.StageName} · {h.ListenedAt:dd.MM.yyyy}"
            }));
        }
    }
}