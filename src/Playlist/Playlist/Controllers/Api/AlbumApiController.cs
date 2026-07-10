using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.ViewModels.Api;

namespace Playlist.Controllers.Api
{
    [ApiController]
    [Route("api/album")]
    [Authorize]
    public class AlbumApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;
        private readonly AlbumRepository _repository;

        public AlbumApiController(MusicBarDbContext context, AlbumRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<AlbumDto>> Get([FromQuery] string? search)
        {
            var query = _context.Albums.Include(a => a.Artist).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(search) || a.Label.ToLower().Contains(search));
            }

            return Ok(query
                .AsNoTracking()
                .ToList()
                .Select(a => a.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<AlbumDto> Get(int id)
        {
            var album = _context.Albums.Include(a => a.Artist).FirstOrDefault(a => a.AlbumId == id);
            if (album == null)
            {
                return NotFound();
            }

            return Ok(album.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<AlbumDto> Post([FromBody] AlbumCreateUpdateDto model)
        {
            var artist = _context.Artists.Find(model.ArtistId);
            if (artist == null)
            {
                return BadRequest("Artist not found.");
            }

            var album = new Album
            {
                AlbumId = _context.Albums.Any() ? _context.Albums.Max(a => a.AlbumId) + 1 : 1,
                Title = model.Title,
                ReleaseDate = model.ReleaseDate,
                Label = model.Label,
                TotalTracks = model.TotalTracks,
                Rating = model.Rating,
                CoverUrl = model.CoverUrl,
                ArtistId = model.ArtistId
            };

            _context.Albums.Add(album);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = album.AlbumId }, album.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<AlbumDto> Put(int id, [FromBody] AlbumCreateUpdateDto model)
        {
            var album = _context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            var artist = _context.Artists.Find(model.ArtistId);
            if (artist == null)
            {
                return BadRequest("Artist not found.");
            }

            album.Title = model.Title;
            album.ReleaseDate = model.ReleaseDate;
            album.Label = model.Label;
            album.TotalTracks = model.TotalTracks;
            album.Rating = model.Rating;
            album.CoverUrl = model.CoverUrl;
            album.ArtistId = model.ArtistId;

            _context.SaveChanges();
            return Ok(album.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var album = _context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            _repository.Delete(id);
            return NoContent();
        }
    }
}
