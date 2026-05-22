using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class FavoriteSongRepository
    {
        private readonly MusicBarDbContext _context;

        public FavoriteSongRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Song> GetFavoriteSongsByUserId(int userId)
        {
            return _context.FavoriteSongs
                .Include(f => f.Song)
                    .ThenInclude(s => s.Artist)
                .Include(f => f.Song)
                    .ThenInclude(s => s.Genre)
                .Include(f => f.Song)
                    .ThenInclude(s => s.Album)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Song)
                .ToList();
        }

        public bool IsFavorite(int userId, int songId)
        {
            return _context.FavoriteSongs
                .Any(f => f.UserId == userId && f.SongId == songId);
        }

        public void Add(int userId, int songId)
        {
            if (IsFavorite(userId, songId))
            {
                return;
            }

            var favorite = new FavoriteSong
            {
                UserId = userId,
                SongId = songId,
                CreatedAt = DateTime.Now
            };

            _context.FavoriteSongs.Add(favorite);
            _context.SaveChanges();
        }

        public void Remove(int userId, int songId)
        {
            var favorite = _context.FavoriteSongs
                .FirstOrDefault(f => f.UserId == userId && f.SongId == songId);

            if (favorite == null)
            {
                return;
            }

            _context.FavoriteSongs.Remove(favorite);
            _context.SaveChanges();
        }
    }
}