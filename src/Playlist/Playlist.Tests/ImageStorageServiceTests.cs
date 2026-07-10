using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Playlist.Services;

namespace Playlist.Tests;

public sealed class ImageStorageServiceTests : IDisposable
{
    // Svaki test koristi svoj privremeni direktorij kako ne bi dirao prave slike.
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "musicbar-image-tests-" + Guid.NewGuid().ToString("N"));

    // Prava PNG slika mora se spremiti, dobiti URL i zatim se moci obrisati.
    [Fact]
    public async Task SaveAsync_SavesAndDeletesValidPng()
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        await using var content = new MemoryStream(bytes);
        var image = new FormFile(content, 0, bytes.Length, "coverImage", "cover.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        var service = new ImageStorageService(new FileStorageOptions(_root));

        var url = await service.SaveAsync(image, "playlist-covers");

        url.Should().StartWith("/uploads/playlist-covers/").And.EndWith(".png");
        var filePath = Path.Combine(_root, "playlist-covers", Path.GetFileName(url));
        File.Exists(filePath).Should().BeTrue();

        service.Delete(url, "playlist-covers");
        File.Exists(filePath).Should().BeFalse();
    }

    // Tekstualna datoteka preimenovana u .jpg mora biti odbijena.
    [Fact]
    public async Task SaveAsync_RejectsFakeImage()
    {
        var bytes = Encoding.UTF8.GetBytes("This is not an image.");
        await using var content = new MemoryStream(bytes);
        var image = new FormFile(content, 0, bytes.Length, "coverImage", "cover.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var service = new ImageStorageService(new FileStorageOptions(_root));

        var action = () => service.SaveAsync(image, "album-covers");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*valid JPG, PNG or WebP*");
    }

    // Nakon testa brisemo samo nas nasumicno stvoren privremeni direktorij.
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
