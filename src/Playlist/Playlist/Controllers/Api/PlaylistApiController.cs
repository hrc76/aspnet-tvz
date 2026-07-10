using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.ViewModels.Api;

namespace Playlist.Controllers.Api
{
    [ApiController]
    [Route("api/playlist")]
    [Authorize]
    public class PlaylistApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;

        public PlaylistApiController(MusicBarDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<PlaylistDto>> Get([FromQuery] string? search)
        {
            var query = _context.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || p.Description.ToLower().Contains(search) || p.Owner.Username.ToLower().Contains(search));
            }

            return Ok(query.AsNoTracking().ToList().Select(p => p.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<PlaylistDto> Get(int id)
        {
            var playlist = _context.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Genre)
                .FirstOrDefault(p => p.PlaylistId == id);

            if (playlist == null)
            {
                return NotFound();
            }

            return Ok(playlist.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<PlaylistDto> Post([FromBody] PlaylistCreateUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Description))
            {
                return BadRequest("Name and description are required.");
            }

            if (!model.OwnerId.HasValue)
            {
                return BadRequest("OwnerId is required.");
            }

            if (!_context.Users.Any(u => u.UserId == model.OwnerId.Value))
            {
                return BadRequest("Owner not found.");
            }

            var songs = _context.Songs.Where(s => model.SongIds.Contains(s.SongId)).ToList();
            var playlist = new Playlist.Models.Playlist
            {
                PlaylistId = _context.Playlists.Any() ? _context.Playlists.Max(p => p.PlaylistId) + 1 : 1,
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow,
                IsPublic = model.IsPublic,
                CoverImageUrl = model.CoverImageUrl,
                Likes = 0,
                OwnerId = model.OwnerId.Value,
                Songs = songs
            };

            _context.Playlists.Add(playlist);
            _context.SaveChanges();

            _context.Entry(playlist).Reference(p => p.Owner).Load();
            _context.Entry(playlist).Collection(p => p.Songs).Query().Include(s => s.Artist).Include(s => s.Genre).Load();

            return CreatedAtAction(nameof(Get), new { id = playlist.PlaylistId }, playlist.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<PlaylistDto> Put(int id, [FromBody] PlaylistCreateUpdateDto model)
        {
            var playlist = _context.Playlists
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistId == id);

            if (playlist == null)
            {
                return NotFound();
            }

            if (model.OwnerId.HasValue && !_context.Users.Any(u => u.UserId == model.OwnerId.Value))
            {
                return BadRequest("Owner not found.");
            }

            playlist.Name = model.Name;
            playlist.Description = model.Description;
            playlist.IsPublic = model.IsPublic;
            playlist.CoverImageUrl = model.CoverImageUrl;
            if (model.OwnerId.HasValue)
            {
                playlist.OwnerId = model.OwnerId.Value;
            }

            var songs = _context.Songs.Where(s => model.SongIds.Contains(s.SongId)).ToList();
            playlist.Songs.Clear();
            foreach (var song in songs)
            {
                playlist.Songs.Add(song);
            }

            _context.SaveChanges();
            _context.Entry(playlist).Reference(p => p.Owner).Load();
            _context.Entry(playlist).Collection(p => p.Songs).Query().Include(s => s.Artist).Include(s => s.Genre).Load();

            return Ok(playlist.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var playlist = _context.Playlists.Find(id);
            if (playlist == null)
            {
                return NotFound();
            }

            _context.Playlists.Remove(playlist);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
