namespace Playlist.Services;

public sealed class FileStorageOptions
{
    public FileStorageOptions(string uploadsRoot)
    {
        UploadsRoot = uploadsRoot;
    }

    public string UploadsRoot { get; }
}
