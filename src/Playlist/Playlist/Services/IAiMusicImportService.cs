using Playlist.ViewModels;

namespace Playlist.Services;

public interface IAiMusicImportService
{
    bool IsConfigured { get; }
    Task<AiImportInterpretation> CreateImportDraftAsync(string prompt, CancellationToken cancellationToken = default);
}
