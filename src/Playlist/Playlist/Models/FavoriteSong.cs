using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class FavoriteSong
    {
        [Key]
        public int FavoriteSongId { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(Song))]
        public int SongId { get; set; }

        public virtual Song Song { get; set; } = null!;
    }
}