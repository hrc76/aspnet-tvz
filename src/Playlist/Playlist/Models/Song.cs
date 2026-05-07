using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int PlayCount { get; set; }

        public double PopularityScore { get; set; }

        public MoodType Mood { get; set; }

        public bool IsExplicit { get; set; }

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