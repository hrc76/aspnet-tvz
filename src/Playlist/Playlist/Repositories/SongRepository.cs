using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class SongRepository
    {
        private readonly MusicBarDbContext _context;

        public SongRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Song> GetAll()
        {
            return _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .ToList();
        }

        public Song? GetById(int id)
        {
            return _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .FirstOrDefault(s => s.SongId == id);
        }
    }
}