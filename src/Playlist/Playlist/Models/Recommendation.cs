namespace Playlist.Models
{
    public class Recommendation
    {
        public int RecommendationId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public double MatchScore { get; set; }

        public User User { get; set; } = null!;
        public Song RecommendedSong { get; set; } = null!;
    }
}