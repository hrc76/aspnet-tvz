namespace Playlist.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string FavoriteGenreName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }

        public List<Playlist> Playlists { get; set; } = new();
        public List<ListeningHistory> ListeningHistory { get; set; } = new();
    }
}