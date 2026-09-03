namespace Playlist.ViewModels;

public sealed class ListeningAnalyticsViewModel
{
    public bool IsGlobal { get; set; }
    public int ProfileCount { get; set; }
    public int TotalPlays { get; set; }
    public int UniqueSongs { get; set; }
    public int ListeningMinutes { get; set; }
    public int ListeningStreak { get; set; }
    public string PeakListeningTime { get; set; } = "No data";
    public List<AnalyticsRankItem> TopSongs { get; set; } = new();
    public List<AnalyticsRankItem> TopArtists { get; set; } = new();
    public List<AnalyticsBarItem> Genres { get; set; } = new();
    public List<AnalyticsBarItem> Moods { get; set; } = new();
    public List<AnalyticsDayItem> LastSevenDays { get; set; } = new();
}

public sealed class AnalyticsRankItem
{
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public int Plays { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class AnalyticsBarItem
{
    public string Name { get; set; } = string.Empty;
    public int Plays { get; set; }
    public int Percentage { get; set; }
}

public sealed class AnalyticsDayItem
{
    public string Label { get; set; } = string.Empty;
    public int Plays { get; set; }
    public int Percentage { get; set; }
}
