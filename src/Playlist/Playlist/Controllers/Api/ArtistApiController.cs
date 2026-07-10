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
    [Route("api/artist")]
    [Authorize]
    public class ArtistApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;
        private readonly ArtistRepository _repository;

        public ArtistApiController(MusicBarDbContext context, ArtistRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<ArtistDto>> Get([FromQuery] string? search)
        {
            var query = _context.Artists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a => a.StageName.ToLower().Contains(search) || a.Country.ToLower().Contains(search));
            }

            return Ok(query
                .AsNoTracking()
                .ToList()
                .Select(a => a.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<ArtistDto> Get(int id)
        {
            var artist = _context.Artists.Find(id);
            if (artist == null)
            {
                return NotFound();
            }

            return Ok(artist.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<ArtistDto> Post([FromBody] ArtistCreateUpdateDto model)
        {
            var artist = new Artist
            {
                ArtistId = _context.Artists.Any() ? _context.Artists.Max(a => a.ArtistId) + 1 : 1,
                StageName = model.StageName,
                Country = model.Country,
                DebutDate = model.DebutDate,
                Biography = model.Biography,
                IsActive = model.IsActive
            };

            _context.Artists.Add(artist);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = artist.ArtistId }, artist.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<ArtistDto> Put(int id, [FromBody] ArtistCreateUpdateDto model)
        {
            var artist = _context.Artists.Find(id);
            if (artist == null)
            {
                return NotFound();
            }

            artist.StageName = model.StageName;
            artist.Country = model.Country;
            artist.DebutDate = model.DebutDate;
            artist.Biography = model.Biography;
            artist.IsActive = model.IsActive;

            _context.SaveChanges();
            return Ok(artist.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var artist = _context.Artists.Find(id);
            if (artist == null)
            {
                return NotFound();
            }

            _repository.Delete(id);
            return NoContent();
        }
    }
}
