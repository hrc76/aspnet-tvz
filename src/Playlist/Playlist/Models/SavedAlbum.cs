using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class SavedAlbum
    {
        [Key]
        public int SavedAlbumId { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(Album))]
        public int AlbumId { get; set; }

        public virtual Album Album { get; set; } = null!;
    }
}