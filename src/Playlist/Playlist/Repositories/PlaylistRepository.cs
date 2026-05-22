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

        public void Add(Playlist.Models.Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            _context.SaveChanges();
        }

        public void Update(Playlist.Models.Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            _context.SaveChanges();
        }

        public void RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var playlist = _context.Playlists
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistId == playlistId);

    if (playlist == null)
    {
        return;
    }

            var song = playlist.Songs.FirstOrDefault(s => s.SongId == songId);

            if (song != null)
            {
                playlist.Songs.Remove(song);
                _context.SaveChanges();
            }
        }

public void Delete(int playlistId)
{
    var playlist = _context.Playlists
        .Include(p => p.Songs)
        .FirstOrDefault(p => p.PlaylistId == playlistId);

    if (playlist == null)
    {
        return;
    }

    playlist.Songs.Clear();
    _context.Playlists.Remove(playlist);
    _context.SaveChanges();
}

        public void AddSongToPlaylist(int playlistId, int songId)
        {
            var playlist = _context.Playlists
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistId == playlistId);

            var song = _context.Songs.FirstOrDefault(s => s.SongId == songId);

    if (playlist == null || song == null)
    {
        return;
    }

    if (!playlist.Songs.Any(s => s.SongId == songId))
    {
        playlist.Songs.Add(song);
        _context.SaveChanges();
    }
}

public List<Playlist.Models.Playlist> GetByOwnerId(int ownerId)
{
    return _context.Playlists
        .Include(p => p.Owner)
        .Include(p => p.Songs)
            .ThenInclude(s => s.Artist)
        .Where(p => p.OwnerId == ownerId)
        .ToList();
}

public void UpdateBasicInfo(int playlistId, string name, string description, bool isPublic)
{
    var playlist = _context.Playlists.FirstOrDefault(p => p.PlaylistId == playlistId);

    if (playlist == null)
    {
        return;
    }

    playlist.Name = name;
    playlist.Description = description;
    playlist.IsPublic = isPublic;

    _context.SaveChanges();
}
    }
}