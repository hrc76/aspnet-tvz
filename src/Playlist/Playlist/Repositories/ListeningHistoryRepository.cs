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
    }
}