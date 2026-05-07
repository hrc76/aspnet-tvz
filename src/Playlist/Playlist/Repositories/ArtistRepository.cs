using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class ArtistRepository
    {
        private readonly MusicBarDbContext _context;

        public ArtistRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Artist> GetAll()
        {
            return _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Songs)
                .ToList();
        }

        public Artist? GetById(int id)
        {
            return _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Songs)
                    .ThenInclude(s => s.Genre)
                .FirstOrDefault(a => a.ArtistId == id);
        }
    }
}