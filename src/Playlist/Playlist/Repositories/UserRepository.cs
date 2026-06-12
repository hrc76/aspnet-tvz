using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class UserRepository
    {
        private readonly MusicBarDbContext _context;

        public UserRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users
                .Include(u => u.ListeningHistory)
                .ThenInclude(h => h.Song)
                .ThenInclude(s => s.Artist)
                .ToList();
        }

        public User? GetById(int id)
        {
            return _context.Users
                .Include(u => u.Playlists)
                .Include(u => u.ListeningHistory)
                .ThenInclude(h => h.Song)
                .ThenInclude(s => s.Artist)
                .FirstOrDefault(u => u.UserId == id);
        }

        public User? GetByEmail(string email)
        {
            return _context.Users
                .Include(u => u.Playlists)
                .Include(u => u.ListeningHistory)
                .ThenInclude(h => h.Song)
                .ThenInclude(s => s.Artist)
                .FirstOrDefault(u => u.Email == email);
        }

        public List<User> Search(string term)
        {
            term = term.ToLower();
            return _context.Users
                .Where(u => u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
                .Take(20)
                .ToList();
        }

        public void Add(User user)
        {
            user.UserId = _context.Users.Any() ? _context.Users.Max(u => u.UserId) + 1 : 1;
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            var existing = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existing == null) return;
            existing.Username = user.Username;
            existing.Email = user.Email;
            existing.RegistrationDate = user.RegistrationDate;
            existing.FavoriteGenreName = user.FavoriteGenreName;
            existing.IsPremium = user.IsPremium;
            _context.SaveChanges();
        }

        public bool UpdateUsername(int userId, string username)
        {
            var existing = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (existing == null) return false;

            existing.Username = username;
            _context.SaveChanges();
            return true;
        }

        public void Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return;
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
