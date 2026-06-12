using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public DateTime ReleaseDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Play count must be 0 or greater.")]
        public int PlayCount { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "Popularity score must be between 0 and 100.")]
        public double PopularityScore { get; set; }

        public MoodType Mood { get; set; }

        public bool IsExplicit { get; set; }

        public string? AudioUrl { get; set; }

        [ForeignKey(nameof(Artist))]
        public int ArtistId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        [ForeignKey(nameof(Album))]
        public int AlbumId { get; set; }

        public virtual Album Album { get; set; } = null!;

        [ForeignKey(nameof(Genre))]
        public int GenreId { get; set; }

        public virtual Genre Genre { get; set; } = null!;

        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}