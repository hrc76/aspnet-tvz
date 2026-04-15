using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class AlbumMockRepository
    {
        private readonly List<Album> _albums;

        public AlbumMockRepository()
        {
            var data = DataSeeder.Seed();
            _albums = data.Albums;
        }

        public List<Album> GetAll()
        {
            return _albums;
        }

        public Album? GetById(int id)
        {
            return _albums.FirstOrDefault(a => a.AlbumId == id);
        }
    }
}