using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Controllers
{
    [Authorize]
    public class ListeningHistoryController : Controller
    {
        private readonly ListeningHistoryRepository _listeningHistoryRepository;
        private readonly UserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ListeningHistoryController> _logger;

        public ListeningHistoryController(ListeningHistoryRepository listeningHistoryRepository,
            UserRepository userRepository, UserManager<AppUser> userManager,
            ILogger<ListeningHistoryController> logger)
        {
            _listeningHistoryRepository = listeningHistoryRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var domainUser = await GetCurrentDomainUserAsync();
            var canSeeAll = User.IsInRole("Admin") || User.IsInRole("Manager");
            var historyItems = canSeeAll
                ? _listeningHistoryRepository.GetAll()
                : domainUser == null ? new List<ListeningHistory>() : _listeningHistoryRepository.GetForUser(domainUser.UserId);
            return View(historyItems);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPlay([FromBody] RecordPlayRequest request, CancellationToken cancellationToken)
        {
            if (request.SongId <= 0) return BadRequest();
            var domainUser = await GetCurrentDomainUserAsync();
            if (domainUser == null) return Unauthorized();

            if (!await _listeningHistoryRepository.RecordPlayAsync(domainUser.UserId, request.SongId, cancellationToken))
                return NotFound();

            _logger.LogInformation("User {UserId} listened to song {SongId} for at least five seconds.",
                domainUser.UserId, request.SongId);
            return Ok(new { recorded = true });
        }

        public async Task<IActionResult> Details(int id)
        {
            var historyItem = _listeningHistoryRepository.GetById(id);

            if (historyItem == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                var domainUser = await GetCurrentDomainUserAsync();
                if (domainUser == null || historyItem.UserId != domainUser.UserId) return Forbid();
            }

            return View(historyItem);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            int? userId = null;
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                userId = (await GetCurrentDomainUserAsync())?.UserId ?? -1;
            var results = _listeningHistoryRepository.Search(term, userId);
            return Json(results.Select(h => new
            {
                id = h.ListeningHistoryId,
                text = h.Song.Title,
                subtitle = $"{h.User.Username} · {h.Song.Artist.StageName} · {h.ListenedAt:dd.MM.yyyy}"
            }));
        }

        private async Task<User?> GetCurrentDomainUserAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            return string.IsNullOrWhiteSpace(appUser?.Email) ? null : _userRepository.GetByEmail(appUser.Email);
        }

        public sealed class RecordPlayRequest
        {
            public int SongId { get; set; }
        }
    }
}
