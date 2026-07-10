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
            existing.CoverUrl = album.CoverUrl;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var album = _context.Albums
                .Include(a => a.Songs)
                    .ThenInclude(song => song.Playlists)
                .FirstOrDefault(a => a.AlbumId == id);
            if (album == null) return;

            var songIds = album.Songs.Select(song => song.SongId).ToList();
            var savedAlbums = _context.SavedAlbums.Where(item => item.AlbumId == id).ToList();
            var favorites = _context.FavoriteSongs.Where(item => songIds.Contains(item.SongId)).ToList();
            var history = _context.ListeningHistories.Where(item => songIds.Contains(item.SongId)).ToList();

            using var transaction = _context.Database.IsRelational()
                ? _context.Database.BeginTransaction()
                : null;
            _context.SavedAlbums.RemoveRange(savedAlbums);
            _context.FavoriteSongs.RemoveRange(favorites);
            _context.ListeningHistories.RemoveRange(history);
            _context.Songs.RemoveRange(album.Songs);
            _context.Albums.Remove(album);
            _context.SaveChanges();
            transaction?.Commit();
        }
    }
}
