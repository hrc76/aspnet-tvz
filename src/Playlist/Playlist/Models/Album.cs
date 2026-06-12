using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Album
    {
        [Key]
        public int AlbumId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Label is required.")]
        [StringLength(200, ErrorMessage = "Label cannot exceed 200 characters.")]
        public string Label { get; set; } = string.Empty;

        [Range(0, 500, ErrorMessage = "Total tracks must be between 0 and 500.")]
        public int TotalTracks { get; set; }

        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0.")]
        public double Rating { get; set; }

        public string? CoverUrl { get; set; }

        [ForeignKey(nameof(Artist))]
        public int ArtistId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}