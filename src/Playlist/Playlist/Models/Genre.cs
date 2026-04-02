namespace Playlist.Models
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<Song> Songs { get; set; } = new();
    }
}