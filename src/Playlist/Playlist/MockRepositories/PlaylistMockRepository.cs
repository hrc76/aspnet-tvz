using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class PlaylistMockRepository
    {
        private readonly List<Models.Playlist> _playlists;

        public PlaylistMockRepository()
        {
            var data = DataSeeder.Seed();
            _playlists = data.Playlists;
        }

        public List<Models.Playlist> GetAll()
        {
            return _playlists;
        }

        public Models.Playlist? GetById(int id)
        {
            return _playlists.FirstOrDefault(p => p.PlaylistId == id);
        }
    }
}