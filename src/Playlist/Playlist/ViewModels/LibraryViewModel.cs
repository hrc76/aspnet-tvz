using Playlist.Models;

namespace Playlist.ViewModels
{
    public class LibraryViewModel
    {
        public List<Song> FavoriteSongs { get; set; } = new();
        public List<LibraryItemViewModel> LibraryItems { get; set; } = new();
    }

    public class LibraryItemViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Album or Playlist
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
    }
}