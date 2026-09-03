namespace Playlist.ViewModels;

public sealed class AchievementViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int Target { get; set; }
    public bool IsUnlocked => Progress >= Target;
    public int Percentage => Target == 0 ? 100 : Math.Min(100, (int)Math.Round(Progress * 100d / Target));
}
