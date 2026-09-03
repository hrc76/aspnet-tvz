using Playlist.ViewModels;

namespace Playlist.Services;

public interface IAiDjService
{
    bool IsConfigured { get; }

    Task<AiDjRecommendation> RecommendAsync(
        string request,
        IReadOnlyCollection<AiDjCatalogSong> catalog,
        AiDjListenerProfile profile,
        CancellationToken cancellationToken = default);
}
