using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class PlaylistAttachment
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey(nameof(Playlist))]
        public int PlaylistId { get; set; }

        public Playlist Playlist { get; set; } = null!;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
