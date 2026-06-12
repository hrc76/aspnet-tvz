using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.ViewModels.Api;

namespace Playlist.Controllers.Api
{
    [ApiController]
    [Route("api/genre")]
    [Authorize]
    public class GenreApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;

        public GenreApiController(MusicBarDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<GenreDto>> Get([FromQuery] string? search)
        {
            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(g => g.Name.ToLower().Contains(search));
            }

            return Ok(query
                .AsNoTracking()
                .ToList()
                .Select(g => g.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<GenreDto> Get(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return NotFound();
            }

            return Ok(genre.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<GenreDto> Post([FromBody] GenreCreateUpdateDto model)
        {
            var genre = new Genre
            {
                GenreId = _context.Genres.Any() ? _context.Genres.Max(g => g.GenreId) + 1 : 1,
                Name = model.Name,
                Description = model.Description
            };

            _context.Genres.Add(genre);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = genre.GenreId }, genre.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<GenreDto> Put(int id, [FromBody] GenreCreateUpdateDto model)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return NotFound();
            }

            genre.Name = model.Name;
            genre.Description = model.Description;
            _context.SaveChanges();

            return Ok(genre.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return NotFound();
            }

            _context.Genres.Remove(genre);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
