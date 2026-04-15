using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class GenreMockRepository
    {
        private readonly List<Genre> _genres;

        public GenreMockRepository()
        {
            var data = DataSeeder.Seed();
            _genres = data.Genres;
        }

        public List<Genre> GetAll()
        {
            return _genres;
        }

        public Genre? GetById(int id)
        {
            return _genres.FirstOrDefault(g => g.GenreId == id);
        }
    }
}