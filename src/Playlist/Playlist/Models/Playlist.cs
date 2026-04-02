namespace Playlist.Models
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public int Likes { get; set; }

        public User Owner { get; set; } = null!;
        public List<Song> Songs { get; set; } = new();
    }
}