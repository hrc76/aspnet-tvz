namespace Playlist.Services;

public interface IImageStorageService
{
    Task<string> SaveAsync(
        IFormFile image,
        string folder,
        string? previousImageUrl = null,
        CancellationToken cancellationToken = default);

    void Delete(string? imageUrl, string folder);
}

public sealed class ImageStorageService : IImageStorageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    // Ekstenzija i MIME tip moraju se slagati, a nize se provjerava i binarni potpis.
    private static readonly IReadOnlyDictionary<string, string> AllowedImages =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly FileStorageOptions _storage;

    public ImageStorageService(FileStorageOptions storage)
    {
        _storage = storage;
    }

    public async Task<string> SaveAsync(
        IFormFile image,
        string folder,
        string? previousImageUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (image == null || image.Length == 0)
        {
            throw new InvalidOperationException("Choose an image before uploading.");
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (image.Length > MaxFileSize
            || !AllowedImages.TryGetValue(extension, out var expectedContentType)
            || !string.Equals(image.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase)
            || !await HasValidSignatureAsync(image, extension, cancellationToken))
        {
            throw new InvalidOperationException("Use a valid JPG, PNG or WebP image smaller than 5 MB.");
        }

        var directory = Path.Combine(_storage.UploadsRoot, folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(directory, fileName);
        await using (var stream = File.Create(filePath))
        {
            await image.CopyToAsync(stream, cancellationToken);
        }

        // Staru sliku brisemo tek nakon sto je nova uspjesno spremljena.
        Delete(previousImageUrl, folder);
        return $"/uploads/{folder}/{fileName}";
    }

    public void Delete(string? imageUrl, string folder)
    {
        var expectedPrefix = $"/uploads/{folder}/";
        if (string.IsNullOrWhiteSpace(imageUrl)
            || !imageUrl.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_storage.UploadsRoot, folder, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile image,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);

        return extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3
                && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8
                && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytesRead >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
