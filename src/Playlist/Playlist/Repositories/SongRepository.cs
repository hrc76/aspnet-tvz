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

        public List<Song> Search(string term)
        {
            term = term.ToLower();
            return _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Where(s => s.Title.ToLower().Contains(term) || s.Artist.StageName.ToLower().Contains(term))
                .Take(20)
                .ToList();
        }

        public List<Song> GetSmartQueue(int seedSongId, IReadOnlyCollection<int> excludedSongIds, int count = 5)
        {
            var seed = _context.Songs.AsNoTracking().FirstOrDefault(song => song.SongId == seedSongId);
            if (seed == null) return new List<Song>();

            return _context.Songs.AsNoTracking()
                .Include(song => song.Artist)
                .Include(song => song.Album)
                .Include(song => song.Genre)
                .Where(song => song.SongId != seedSongId && !excludedSongIds.Contains(song.SongId))
                .OrderByDescending(song => (song.GenreId == seed.GenreId ? 4 : 0)
                    + (song.Mood == seed.Mood ? 2 : 0)
                    + (song.ArtistId == seed.ArtistId ? 1 : 0))
                .ThenByDescending(song => song.PopularityScore)
                .Take(count)
                .ToList();
        }

        public void Add(Song song)
        {
            song.SongId = _context.Songs.Any() ? _context.Songs.Max(s => s.SongId) + 1 : 1;
            _context.Songs.Add(song);
            _context.SaveChanges();
        }

        public void Update(Song song)
        {
            var existing = _context.Songs.FirstOrDefault(s => s.SongId == song.SongId);
            if (existing == null) return;
            existing.Title = song.Title;
            existing.Duration = song.Duration;
            existing.ReleaseDate = song.ReleaseDate;
            existing.PlayCount = song.PlayCount;
            existing.PopularityScore = song.PopularityScore;
            existing.Mood = song.Mood;
            existing.IsExplicit = song.IsExplicit;
            existing.ArtistId = song.ArtistId;
            existing.AlbumId = song.AlbumId;
            existing.GenreId = song.GenreId;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var song = _context.Songs.Include(s => s.Playlists).FirstOrDefault(s => s.SongId == id);
            if (song == null) return;
            var favorites = _context.FavoriteSongs.Where(x => x.SongId == id).ToList();
            var history = _context.ListeningHistories.Where(x => x.SongId == id).ToList();
            using var transaction = _context.Database.IsRelational() ? _context.Database.BeginTransaction() : null;
            song.Playlists.Clear();
            _context.FavoriteSongs.RemoveRange(favorites);
            _context.ListeningHistories.RemoveRange(history);
            _context.Songs.Remove(song);
            _context.SaveChanges();
            transaction?.Commit();
        }
    }
}
