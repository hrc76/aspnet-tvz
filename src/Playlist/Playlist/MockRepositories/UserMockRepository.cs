using Playlist.Models;

namespace Playlist.MockRepositories
{
    public class UserMockRepository
    {
        private readonly List<User> _users;

        public UserMockRepository()
        {
            var data = DataSeeder.Seed();
            _users = data.Users;
        }

        public List<User> GetAll()
        {
            return _users;
        }

        public User? GetById(int id)
        {
            return _users.FirstOrDefault(u => u.UserId == id);
        }
    }
}