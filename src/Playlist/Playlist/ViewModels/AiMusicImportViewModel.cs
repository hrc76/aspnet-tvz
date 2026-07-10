using System.ComponentModel.DataAnnotations;

namespace Playlist.ViewModels;

public sealed class AiMusicImportViewModel
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Prompt { get; set; } = string.Empty;

    public AiSongDraft? Draft { get; set; }
    public AiArtistDraft? ArtistDraft { get; set; }
    public AiAlbumDraft? AlbumDraft { get; set; }
    public List<MusicMetadataCandidate> MetadataCandidates { get; set; } = new();
    public List<string> GenreNames { get; set; } = new();
    public bool ApiConfigured { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class AiImportInterpretation
{
    public string EntityType { get; set; } = "Song";
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string AlbumTitle { get; set; } = string.Empty;
    public string GenreName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Mood { get; set; } = "Energetic";
    public bool IsExplicit { get; set; }
    public string Country { get; set; } = "Unknown";
    public string Biography { get; set; } = "Unknown";
    public string Label { get; set; } = "Unknown";
    public int TotalTracks { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AiArtistDraft
{
    [Required, StringLength(200)]
    public string StageName { get; set; } = "Unknown artist";

    [Required, StringLength(100)]
    public string Country { get; set; } = "Unknown";

    public DateTime DebutDate { get; set; }

    [StringLength(2000)]
    public string Biography { get; set; } = "Unknown";

    public bool IsActive { get; set; } = true;
}

public sealed class AiAlbumDraft
{
    [Required, StringLength(200)]
    public string Title { get; set; } = "Unknown album";

    [Required, StringLength(200)]
    public string ArtistName { get; set; } = "Unknown artist";

    public DateTime ReleaseDate { get; set; }

    [Required, StringLength(200)]
    public string Label { get; set; } = "Unknown";

    [Range(0, 500)]
    public int TotalTracks { get; set; }
}

public sealed class MusicMetadataCandidate
{
    public string MusicBrainzId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string AlbumTitle { get; set; } = string.Empty;
    public string GenreName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public DateTime? ReleaseDate { get; set; }
}

public sealed class AiSongDraft
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ArtistName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string AlbumTitle { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string GenreName { get; set; } = string.Empty;

    [Range(1, 7200)]
    public int DurationSeconds { get; set; }

    public DateTime ReleaseDate { get; set; }

    [Required]
    public string Mood { get; set; } = "Energetic";

    public bool IsExplicit { get; set; }
}
