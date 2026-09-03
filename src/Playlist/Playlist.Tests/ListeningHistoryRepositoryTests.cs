using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.Repositories;

namespace Playlist.Tests;

public class ListeningHistoryRepositoryTests
{
    // Povijest cuva samo zadnjih 20 slusanja svakog korisnika.
    [Fact]
    public async Task RecordPlay_KeepsLatestTwentyItemsPerUser()
    {
        var options = new DbContextOptionsBuilder<MusicBarDbContext>()
            .UseInMemoryDatabase($"history-{Guid.NewGuid():N}")
            .Options;
        await using var context = new MusicBarDbContext(options);
        var user = new User
        {
            Username = "history-test",
            Email = "history-test@example.com",
            RegistrationDate = DateTime.UtcNow,
            FavoriteGenreName = "Rock"
        };
        var song = new Song { Title = "History Test Song" };
        context.Users.Add(user);
        context.Songs.Add(song);
        await context.SaveChangesAsync();

        var repository = new ListeningHistoryRepository(context);
        for (var index = 0; index < 21; index++)
            (await repository.RecordPlayAsync(user.UserId, song.SongId, CancellationToken.None)).Should().BeTrue();

        var history = await context.ListeningHistories
            .Where(item => item.UserId == user.UserId)
            .OrderBy(item => item.ListeningHistoryId)
            .ToListAsync();
        history.Should().HaveCount(20);
        history.First().ListeningHistoryId.Should().Be(2);
        song.PlayCount.Should().Be(21);
    }
}
