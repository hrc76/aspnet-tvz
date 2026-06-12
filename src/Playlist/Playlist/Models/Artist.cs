using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        [Required(ErrorMessage = "Stage name is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Stage name must be between 1 and 200 characters.")]
        public string StageName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        public DateTime DebutDate { get; set; }

        [StringLength(2000, ErrorMessage = "Biography cannot exceed 2000 characters.")]
        public string Biography { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}