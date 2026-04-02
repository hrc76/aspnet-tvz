namespace Playlist.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime DebutDate { get; set; }
        public string Biography { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public List<Album> Albums { get; set; } = new();
        public List<Song> Songs { get; set; } = new();
    }
}