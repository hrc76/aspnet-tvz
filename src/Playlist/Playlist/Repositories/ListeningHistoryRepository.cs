using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class ListeningHistoryRepository
    {
        private readonly MusicBarDbContext _context;

        public ListeningHistoryRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<ListeningHistory> GetAll()
        {
            return _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .ToList();
        }

        public ListeningHistory? GetById(int id)
        {
            return _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .FirstOrDefault(h => h.ListeningHistoryId == id);
        }

        public List<ListeningHistory> Search(string term)
        {
            var lower = term.ToLower();
            return _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .Where(h => h.User.Username.ToLower().Contains(lower)
                         || h.Song.Title.ToLower().Contains(lower)
                         || h.Song.Artist.StageName.ToLower().Contains(lower))
                .Take(20)
                .ToList();
        }
    }
}