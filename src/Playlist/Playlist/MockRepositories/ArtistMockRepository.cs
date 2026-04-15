using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class ArtistMockRepository
    {
        private readonly List<Artist> _artists;

        public ArtistMockRepository()
        {
            var data = DataSeeder.Seed();
            _artists = data.Artists;
        }

        public List<Artist> GetAll()
        {
            return _artists;
        }

        public Artist? GetById(int id)
        {
            return _artists.FirstOrDefault(a => a.ArtistId == id);
        }
    }
}