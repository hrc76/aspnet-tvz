using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Playlist.Data;
using Playlist.Models;
using Playlist.Repositories;
using System.IO;

namespace Playlist.Controllers
{
    [Authorize]
    public class PlaylistController : Controller
    {
        private readonly PlaylistRepository _playlistRepository;
        private readonly MusicBarDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public PlaylistController(
            PlaylistRepository playlistRepository,
            MusicBarDbContext dbContext,
            UserManager<AppUser> userManager,
            UserRepository userRepository)
        {
            _playlistRepository = playlistRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var playlists = _playlistRepository.GetAll();
            return View(playlists);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var playlist = _playlistRepository.GetById(id);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAttachments(int playlistId)
        {
            var attachments = _dbContext.PlaylistAttachments
                .Where(a => a.PlaylistId == playlistId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return PartialView("_PlaylistAttachmentList", attachments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadAttachment(int playlistId, IFormFile file)
        {
            var playlist = _dbContext.Playlists.Find(playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest();
            }

            var uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "playlists",
                playlistId.ToString());

            Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new PlaylistAttachment
            {
                PlaylistId = playlistId,
                FileName = file.FileName,
                FilePath = "/uploads/playlists/" + playlistId + "/" + fileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.PlaylistAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteAttachment(int id)
        {
            var attachment = _dbContext.PlaylistAttachments.Find(id);
            if (attachment == null)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _dbContext.PlaylistAttachments.Remove(attachment);
            _dbContext.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Playlist.Models.Playlist playlist)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Owner");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                return View(playlist);
            }

            playlist.OwnerId = userId.Value;
            playlist.CreatedAt = DateTime.Now;
            playlist.Likes = 0;
            playlist.Songs = new List<Song>();

            _playlistRepository.Add(playlist);

            return RedirectToAction("Index", "Library");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var userId = await GetCurrentDomainUserIdAsync();
            var playlist = _playlistRepository.GetById(playlistId);
            if (userId == null || playlist == null || playlist.OwnerId != userId.Value)
            {
                return Forbid();
            }

            _playlistRepository.AddSongToPlaylist(playlistId, songId);
            return RedirectToAction("Details", "Song", new { id = songId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSong(int playlistId, int songId)
        {
            _playlistRepository.RemoveSongFromPlaylist(playlistId, songId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _playlistRepository.Delete(id);
            return RedirectToAction("Index", "Library");
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            term = term?.ToLower() ?? "";
            var result = _playlistRepository.GetAll()
                .Where(p => p.Name.ToLower().Contains(term) || (p.Owner != null && p.Owner.Username.ToLower().Contains(term)))
                .Take(12)
                .Select(p => new {
                    id       = p.PlaylistId,
                    text     = p.Name,
                    subtitle = $"by {p.Owner.Username} · {p.Songs.Count} songs"
                })
                .ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUserPlaylists(string term)
        {
            term = term?.ToLower() ?? "";
            var userId = await GetCurrentDomainUserIdAsync();

            var playlists = userId == null
                ? _playlistRepository.GetAll()
                : _playlistRepository.GetByOwnerId(userId.Value);

            var result = playlists
                .Where(p => p.Name.ToLower().Contains(term))
                .Take(10)
                .Select(p => new
                {
                    id = p.PlaylistId,
                    text = p.Name,
                    subtitle = $"{p.Songs.Count} songs · {(p.IsPublic ? "Public" : "Private")}"
                })
                .ToList();

            return Json(result);
        }

        public IActionResult Edit(int id)
        {
            var playlist = _playlistRepository.GetById(id);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Playlist.Models.Playlist playlist)
        {
            if (id != playlist.PlaylistId)
            {
                return NotFound();
            }

            ModelState.Remove("Owner");
            ModelState.Remove("Songs");

            if (!ModelState.IsValid)
            {
                return View(playlist);
            }

            _playlistRepository.UpdateBasicInfo(
                playlist.PlaylistId,
                playlist.Name,
                playlist.Description,
                playlist.IsPublic
            );

            return RedirectToAction(nameof(Details), new { id = playlist.PlaylistId });
        }

        private async Task<int?> GetCurrentDomainUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(User);
                if (appUser != null)
                {
                    var domainUser = _userRepository.GetByEmail(appUser.Email ?? string.Empty);
                    if (domainUser != null)
                    {
                        return domainUser.UserId;
                    }

                    var newDomainUser = new User
                    {
                        Username = appUser.UserName ?? appUser.Email ?? "Guest",
                        Email = appUser.Email ?? string.Empty,
                        RegistrationDate = DateTime.UtcNow,
                        IsPremium = false
                    };
                    _userRepository.Add(newDomainUser);
                    return newDomainUser.UserId;
                }
            }

            return HttpContext.Session.GetInt32("UserId");
        }
    }
}
