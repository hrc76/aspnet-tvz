namespace Playlist.Models
{
    public class Song
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int PlayCount { get; set; }
        public double PopularityScore { get; set; }
        public MoodType Mood { get; set; }
        public bool IsExplicit { get; set; }

        public Artist Artist { get; set; } = null!;
        public Album Album { get; set; } = null!;
        public Genre Genre { get; set; } = null!;

        public List<Playlist> Playlists { get; set; } = new();
    }
}