using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        [StringLength(100, ErrorMessage = "Favorite genre cannot exceed 100 characters.")]
        public string FavoriteGenreName { get; set; } = string.Empty;

        public bool IsPremium { get; set; }

        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

        public virtual ICollection<ListeningHistory> ListeningHistory { get; set; } = new List<ListeningHistory>();

        public virtual ICollection<FavoriteSong> FavoriteSongs { get; set; } = new List<FavoriteSong>();
        public virtual ICollection<SavedAlbum> SavedAlbums { get; set; } = new List<SavedAlbum>();
    }
}