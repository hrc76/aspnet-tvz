using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Repositories
{
    public class GenreRepository
    {
        private readonly MusicBarDbContext _context;

        public GenreRepository(MusicBarDbContext context)
        {
            _context = context;
        }

        public List<Genre> GetAll()
        {
            return _context.Genres
                .Include(g => g.Songs)
                .ToList();
        }

        public Genre? GetById(int id)
        {
            return _context.Genres
                .Include(g => g.Songs)
                    .ThenInclude(s => s.Artist)
                .FirstOrDefault(g => g.GenreId == id);
        }
    }
}