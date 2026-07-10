using Playlist.ViewModels;

namespace Playlist.Services;

public interface IMusicMetadataService
{
    Task<IReadOnlyList<MusicMetadataCandidate>> SearchRecordingsAsync(
        string title,
        CancellationToken cancellationToken = default);
}
