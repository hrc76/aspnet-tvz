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

        public List<Album> Search(string term)
        {
            term = term.ToLower();
            return _context.Albums
                .Include(a => a.Artist)
                .Where(a => a.Title.ToLower().Contains(term) || a.Artist.StageName.ToLower().Contains(term) || a.Label.ToLower().Contains(term))
                .Take(20)
                .ToList();
        }

        public void Add(Album album)
        {
            album.AlbumId = _context.Albums.Any() ? _context.Albums.Max(a => a.AlbumId) + 1 : 1;
            _context.Albums.Add(album);
            _context.SaveChanges();
        }

        public void Update(Album album)
        {
            var existing = _context.Albums.FirstOrDefault(a => a.AlbumId == album.AlbumId);
            if (existing == null) return;
            existing.Title = album.Title;
            existing.ReleaseDate = album.ReleaseDate;
            existing.Label = album.Label;
            existing.TotalTracks = album.TotalTracks;
            existing.Rating = album.Rating;
            existing.ArtistId = album.ArtistId;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var album = _context.Albums.FirstOrDefault(a => a.AlbumId == id);
            if (album == null) return;
            _context.Albums.Remove(album);
            _context.SaveChanges();
        }
    }
}