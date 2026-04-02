namespace Playlist.Models
{
    public class Album
    {
        public int AlbumId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string Label { get; set; } = string.Empty;
        public int TotalTracks { get; set; }
        public double Rating { get; set; }

        public Artist Artist { get; set; } = null!;
        public List<Song> Songs { get; set; } = new();
    }
}