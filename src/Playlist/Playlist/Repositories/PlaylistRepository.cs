using Microsoft.EntityFrameworkCore;
using Playlist.Data;

namespace Playlist.Repositories
{
    public class PlaylistRepository
    {
        private readonly MusicBarDbContext _context;

        public PlaylistRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Playlist.Models.Playlist> GetAll()
        {
            return _context.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Genre)
                .ToList();
        }

        public Playlist.Models.Playlist? GetById(int id)
        {
            return _context.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Genre)
                .FirstOrDefault(p => p.PlaylistId == id);
        }
    }
}