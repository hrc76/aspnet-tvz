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

        public List<Artist> Search(string term)
        {
            term = term.ToLower();
            return _context.Artists
                .Where(a => a.StageName.ToLower().Contains(term) || a.Country.ToLower().Contains(term))
                .Take(20)
                .ToList();
        }

        public void Add(Artist artist)
        {
            artist.ArtistId = _context.Artists.Any() ? _context.Artists.Max(a => a.ArtistId) + 1 : 1;
            _context.Artists.Add(artist);
            _context.SaveChanges();
        }

        public void Update(Artist artist)
        {
            var existing = _context.Artists.FirstOrDefault(a => a.ArtistId == artist.ArtistId);
            if (existing == null) return;
            existing.StageName = artist.StageName;
            existing.Country = artist.Country;
            existing.DebutDate = artist.DebutDate;
            existing.Biography = artist.Biography;
            existing.IsActive = artist.IsActive;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var artist = _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Songs)
                    .ThenInclude(song => song.Playlists)
                .FirstOrDefault(a => a.ArtistId == id);
            if (artist == null) return;
            var songIds = artist.Songs.Select(song => song.SongId).ToList();
            var albumIds = artist.Albums.Select(album => album.AlbumId).ToList();
            var favorites = _context.FavoriteSongs.Where(x => songIds.Contains(x.SongId)).ToList();
            var history = _context.ListeningHistories.Where(x => songIds.Contains(x.SongId)).ToList();
            var savedAlbums = _context.SavedAlbums.Where(x => albumIds.Contains(x.AlbumId)).ToList();
            using var transaction = _context.Database.IsRelational() ? _context.Database.BeginTransaction() : null;
            _context.FavoriteSongs.RemoveRange(favorites);
            _context.ListeningHistories.RemoveRange(history);
            _context.SavedAlbums.RemoveRange(savedAlbums);
            _context.Songs.RemoveRange(artist.Songs);
            _context.Albums.RemoveRange(artist.Albums);
            _context.Artists.Remove(artist);
            _context.SaveChanges();
            transaction?.Commit();
        }
    }
}
