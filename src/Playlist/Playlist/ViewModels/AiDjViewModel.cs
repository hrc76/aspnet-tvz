using System.ComponentModel.DataAnnotations;
using Playlist.Models;

namespace Playlist.ViewModels;

public sealed class AiDjViewModel
{
    [Required(ErrorMessage = "Tell the AI DJ what you want to hear.")]
    [StringLength(500, MinimumLength = 3)]
    public string Request { get; set; } = string.Empty;

    public bool ApiConfigured { get; set; }
    public AiDjRecommendation? Recommendation { get; set; }
    public List<Song> Songs { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public sealed class AiDjRecommendation
{
    public string PlaylistName { get; set; } = "AI Mix";
    public string Description { get; set; } = "A personalized AI DJ selection.";
    public string Explanation { get; set; } = string.Empty;
    public List<int> SongIds { get; set; } = new();
}

public sealed class AiDjCatalogSong
{
    public int SongId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Mood { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public bool IsExplicit { get; set; }
    public double Popularity { get; set; }
}

public sealed class AiDjListenerProfile
{
    public string FavoriteGenre { get; set; } = "Not selected";
    public List<int> FavoriteSongIds { get; set; } = new();
    public List<int> SavedAlbumIds { get; set; } = new();
    public Dictionary<int, int> ListeningCounts { get; set; } = new();
}

public sealed class AiDjSaveRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string PlaylistName { get; set; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }
    public List<int> SongIds { get; set; } = new();
}
