using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class AlbumRepository
    {
        private readonly MusicBarDbContext _context;

        public AlbumRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Album> GetAll()
        {
            return _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                .ToList();
        }

        public Album? GetById(int id)
        {
            return _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(a => a.Songs)
                    .ThenInclude(s => s.Genre)
                .FirstOrDefault(a => a.AlbumId == id);
        }
    }
}