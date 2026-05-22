using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        public string FavoriteGenreName { get; set; } = string.Empty;

        public bool IsPremium { get; set; }

        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

        public virtual ICollection<ListeningHistory> ListeningHistory { get; set; } = new List<ListeningHistory>();

        public virtual ICollection<FavoriteSong> FavoriteSongs { get; set; } = new List<FavoriteSong>();
        public virtual ICollection<SavedAlbum> SavedAlbums { get; set; } = new List<SavedAlbum>();
    }
}