using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}