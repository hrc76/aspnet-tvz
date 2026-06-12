using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.ViewModels.Api;

namespace Playlist.Controllers.Api
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserApiController : ControllerBase
    {
        private readonly MusicBarDbContext _context;

        public UserApiController(MusicBarDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get([FromQuery] string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
            }

            return Ok(query
                .AsNoTracking()
                .ToList()
                .Select(u => u.ToDto()));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<UserDto> Get(int id)
        {
            var user = _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<UserDto> Post([FromBody] UserCreateUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Username and email are required.");
            }

            var user = new User
            {
                UserId = _context.Users.Any() ? _context.Users.Max(u => u.UserId) + 1 : 1,
                Username = model.Username,
                Email = model.Email,
                RegistrationDate = model.RegistrationDate,
                FavoriteGenreName = model.FavoriteGenreName,
                IsPremium = model.IsPremium
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = user.UserId }, user.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<UserDto> Put(int id, [FromBody] UserCreateUpdateDto model)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = model.Username;
            user.Email = model.Email;
            user.RegistrationDate = model.RegistrationDate;
            user.FavoriteGenreName = model.FavoriteGenreName;
            user.IsPremium = model.IsPremium;

            _context.SaveChanges();
            return Ok(user.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
