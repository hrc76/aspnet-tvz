namespace Playlist.Models
{
    public class ListeningHistory
    {
        public int ListeningHistoryId { get; set; }
        public DateTime ListenedAt { get; set; }
        public int Repeats { get; set; }

        public User User { get; set; } = null!;
        public Song Song { get; set; } = null!;
    }
}