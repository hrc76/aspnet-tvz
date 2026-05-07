using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class ListeningHistory
    {
        [Key]
        public int ListeningHistoryId { get; set; }

        public DateTime ListenedAt { get; set; }

        public int Repeats { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(Song))]
        public int SongId { get; set; }

        public virtual Song Song { get; set; } = null!;
    }
}