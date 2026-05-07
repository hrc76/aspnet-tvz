using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Album
    {
        [Key]
        public int AlbumId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        public string Label { get; set; } = string.Empty;

        public int TotalTracks { get; set; }

        public double Rating { get; set; }

        [ForeignKey(nameof(Artist))]
        public int ArtistId { get; set; }

        public virtual Artist Artist { get; set; } = null!;

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}