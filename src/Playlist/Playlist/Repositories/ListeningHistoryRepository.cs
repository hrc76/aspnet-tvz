using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class ListeningHistoryRepository
    {
        // History je pomični prozor: nova pjesma ulazi, a najstarija ispada nakon 20 zapisa.
        private const int MaximumHistoryItemsPerUser = 20;
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
                .OrderByDescending(h => h.ListenedAt)
                .ToList();
        }

        public List<ListeningHistory> GetForUser(int userId)
        {
            return _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ListenedAt)
                .ToList();
        }

        public async Task<bool> RecordPlayAsync(int userId, int songId, CancellationToken cancellationToken)
        {
            var song = await _context.Songs.FindAsync(new object[] { songId }, cancellationToken);
            if (song == null) return false;

            // Seed koristi eksplicitne kljuceve pa i nove history zapise numeriramo rucno.
            var nextHistoryId = await _context.ListeningHistories.AnyAsync(cancellationToken)
                ? await _context.ListeningHistories.MaxAsync(item => item.ListeningHistoryId, cancellationToken) + 1
                : 1;

            var existingCount = await _context.ListeningHistories
                .CountAsync(item => item.UserId == userId, cancellationToken);
            var itemsToRemove = existingCount - MaximumHistoryItemsPerUser + 1;
            if (itemsToRemove > 0)
            {
                var oldestItems = await _context.ListeningHistories
                    .Where(item => item.UserId == userId)
                    .OrderBy(item => item.ListenedAt)
                    .ThenBy(item => item.ListeningHistoryId)
                    .Take(itemsToRemove)
                    .ToListAsync(cancellationToken);
                _context.ListeningHistories.RemoveRange(oldestItems);
            }

            _context.ListeningHistories.Add(new ListeningHistory
            {
                ListeningHistoryId = nextHistoryId,
                UserId = userId,
                SongId = songId,
                ListenedAt = DateTime.UtcNow,
                Repeats = 1
            });
            song.PlayCount++;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public ListeningHistory? GetById(int id)
        {
            return _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .FirstOrDefault(h => h.ListeningHistoryId == id);
        }

        public List<ListeningHistory> Search(string term, int? userId = null)
        {
            var lower = term.ToLower();
            var query = _context.ListeningHistories
                .Include(h => h.User)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)
                .AsQueryable();
            if (userId.HasValue) query = query.Where(h => h.UserId == userId.Value);
            return query
                .Where(h => h.User.Username.ToLower().Contains(lower)
                         || h.Song.Title.ToLower().Contains(lower)
                         || h.Song.Artist.StageName.ToLower().Contains(lower))
                .OrderByDescending(h => h.ListenedAt)
                .Take(20)
                .ToList();
        }
    }
}
