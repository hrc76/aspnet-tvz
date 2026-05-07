using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsPublic { get; set; }

        public int Likes { get; set; }

        [ForeignKey(nameof(Owner))]
        public int OwnerId { get; set; }

        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}