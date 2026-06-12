using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playlist.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [Required(ErrorMessage = "Playlist name is required.")]
        [StringLength(80,
            MinimumLength = 2,
            ErrorMessage = "Playlist name must be between 2 and 80 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250,
            MinimumLength = 5,
            ErrorMessage = "Description must be between 5 and 250 characters.")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsPublic { get; set; }

        public int Likes { get; set; }

        [ForeignKey(nameof(Owner))]
        public int OwnerId { get; set; }

        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<PlaylistAttachment> Attachments { get; set; } = new List<PlaylistAttachment>();
    }
}