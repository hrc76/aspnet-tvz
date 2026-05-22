using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class SavedAlbumRepository
    {
        private readonly MusicBarDbContext _context;

        public SavedAlbumRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Album> GetSavedAlbumsByUserId(int userId)
        {
            return _context.SavedAlbums
                .Include(s => s.Album)
                    .ThenInclude(a => a.Artist)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.Album)
                .ToList();
        }

        public bool IsSaved(int userId, int albumId)
        {
            return _context.SavedAlbums
                .Any(s => s.UserId == userId && s.AlbumId == albumId);
        }

        public void Add(int userId, int albumId)
        {
            if (IsSaved(userId, albumId))
            {
                return;
            }

            _context.SavedAlbums.Add(new SavedAlbum
            {
                UserId = userId,
                AlbumId = albumId,
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
        }

        public void Remove(int userId, int albumId)
        {
            var savedAlbum = _context.SavedAlbums
                .FirstOrDefault(s => s.UserId == userId && s.AlbumId == albumId);

            if (savedAlbum == null)
            {
                return;
            }

            _context.SavedAlbums.Remove(savedAlbum);
            _context.SaveChanges();
        }
    }
}