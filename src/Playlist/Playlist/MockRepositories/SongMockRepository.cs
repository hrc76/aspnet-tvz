using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class SongMockRepository
    {
        private readonly List<Song> _songs;

        public SongMockRepository()
        {
            var data = DataSeeder.Seed();
            _songs = data.Songs;
        }

        public List<Song> GetAll()
        {
            return _songs;
        }

        public Song? GetById(int id)
        {
            return _songs.FirstOrDefault(s => s.SongId == id);
        }
    }
}