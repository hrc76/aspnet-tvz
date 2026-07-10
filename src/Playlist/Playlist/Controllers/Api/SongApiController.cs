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
    [Route("api/song")]
    [Authorize]
    public class SongApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;
        private readonly SongRepository _repository;

        public SongApiController(MusicBarDbContext context, SongRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<SongDto>> Get([FromQuery] string? search)
        {
            var query = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s => s.Title.ToLower().Contains(search) || s.Artist.StageName.ToLower().Contains(search) || s.Genre.Name.ToLower().Contains(search));
            }

            return Ok(query
                .AsNoTracking()
                .ToList()
                .Select(s => s.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<SongDto> Get(int id)
        {
            var song = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .FirstOrDefault(s => s.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            return Ok(song.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<SongDto> Post([FromBody] SongCreateUpdateDto model)
        {
            if (!_context.Artists.Any(a => a.ArtistId == model.ArtistId) || !_context.Albums.Any(a => a.AlbumId == model.AlbumId) || !_context.Genres.Any(g => g.GenreId == model.GenreId))
            {
                return BadRequest("Artist, album, or genre not found.");
            }

            var song = new Song
            {
                SongId = _context.Songs.Any() ? _context.Songs.Max(s => s.SongId) + 1 : 1,
                Title = model.Title,
                Duration = model.Duration,
                ReleaseDate = model.ReleaseDate,
                PlayCount = model.PlayCount,
                PopularityScore = model.PopularityScore,
                Mood = model.Mood,
                IsExplicit = model.IsExplicit,
                AudioUrl = model.AudioUrl,
                ArtistId = model.ArtistId,
                AlbumId = model.AlbumId,
                GenreId = model.GenreId
            };

            _context.Songs.Add(song);
            _context.SaveChanges();
            _context.Entry(song).Reference(s => s.Artist).Load();
            _context.Entry(song).Reference(s => s.Album).Load();
            _context.Entry(song).Reference(s => s.Genre).Load();

            return CreatedAtAction(nameof(Get), new { id = song.SongId }, song.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<SongDto> Put(int id, [FromBody] SongCreateUpdateDto model)
        {
            var song = _context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }

            if (!_context.Artists.Any(a => a.ArtistId == model.ArtistId) || !_context.Albums.Any(a => a.AlbumId == model.AlbumId) || !_context.Genres.Any(g => g.GenreId == model.GenreId))
            {
                return BadRequest("Artist, album, or genre not found.");
            }

            song.Title = model.Title;
            song.Duration = model.Duration;
            song.ReleaseDate = model.ReleaseDate;
            song.PlayCount = model.PlayCount;
            song.PopularityScore = model.PopularityScore;
            song.Mood = model.Mood;
            song.IsExplicit = model.IsExplicit;
            song.AudioUrl = model.AudioUrl;
            song.ArtistId = model.ArtistId;
            song.AlbumId = model.AlbumId;
            song.GenreId = model.GenreId;

            _context.SaveChanges();
            _context.Entry(song).Reference(s => s.Artist).Load();
            _context.Entry(song).Reference(s => s.Album).Load();
            _context.Entry(song).Reference(s => s.Genre).Load();

            return Ok(song.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var song = _context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }

            _repository.Delete(id);
            return NoContent();
        }
    }
}
