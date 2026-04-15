using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class ListeningHistoryMockRepository
    {
        private readonly List<ListeningHistory> _listeningHistories;

        public ListeningHistoryMockRepository()
        {
            var data = DataSeeder.Seed();
            _listeningHistories = data.ListeningHistories;
        }

        public List<ListeningHistory> GetAll()
        {
            return _listeningHistories;
        }

        public ListeningHistory? GetById(int id)
        {
            return _listeningHistories.FirstOrDefault(h => h.ListeningHistoryId == id);
        }
    }
}