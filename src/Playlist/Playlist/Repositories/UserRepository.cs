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
                .Include(u => u.Playlists)
                .Include(u => u.ListeningHistory)
                .ToList();
        }

        public User? GetById(int id)
        {
            return _context.Users
                .Include(u => u.Playlists)
                .Include(u => u.ListeningHistory)
                    .ThenInclude(h => h.Song)
                .FirstOrDefault(u => u.UserId == id);
        }
    }
}