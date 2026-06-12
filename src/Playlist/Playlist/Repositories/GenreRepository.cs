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

        public List<Genre> Search(string term)
        {
            term = term.ToLower();
            return _context.Genres
                .Where(g => g.Name.ToLower().Contains(term) || g.Description.ToLower().Contains(term))
                .Take(20)
                .ToList();
        }

        public void Add(Genre genre)
        {
            genre.GenreId = _context.Genres.Any() ? _context.Genres.Max(g => g.GenreId) + 1 : 1;
            _context.Genres.Add(genre);
            _context.SaveChanges();
        }

        public void Update(Genre genre)
        {
            var existing = _context.Genres.FirstOrDefault(g => g.GenreId == genre.GenreId);
            if (existing == null) return;
            existing.Name = genre.Name;
            existing.Description = genre.Description;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var genre = _context.Genres.FirstOrDefault(g => g.GenreId == id);
            if (genre == null) return;
            _context.Genres.Remove(genre);
            _context.SaveChanges();
        }
    }
}