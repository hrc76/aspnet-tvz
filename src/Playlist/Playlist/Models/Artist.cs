using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        public string StageName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public DateTime DebutDate { get; set; }

        public string Biography { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}