using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Playlist.Models;
using Playlist.ViewModels.Api;
using Xunit;

namespace Playlist.Tests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> ApiRoutes()
    {
        yield return new object[] { "/api/artist" };
        yield return new object[] { "/api/album" };
        yield return new object[] { "/api/genre" };
        yield return new object[] { "/api/song" };
        yield return new object[] { "/api/playlist" };
        yield return new object[] { "/api/user" };
    }

    [Theory]
    [MemberData(nameof(ApiRoutes))]
    public async Task GetAll_ReturnsOk_ForEveryApiController(string route)
    {
        var response = await _factory.CreateClient().GetAsync(route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(ApiRoutes))]
    public async Task GetById_ReturnsNotFound_ForUnknownId(string route)
    {
        var response = await _factory.CreateClient().GetAsync($"{route}/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutAuthentication()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Test-Auth", "none");

        var response = await client.PostAsJsonAsync("/api/genre", ValidGenre("Unauthorized genre"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Artist_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var create = new ArtistCreateUpdateDto
        {
            StageName = Unique("Integration Artist"),
            Country = "Croatia",
            DebutDate = new DateTime(2020, 1, 1),
            Biography = "Created by an integration test.",
            IsActive = true
        };

        var post = await client.PostAsJsonAsync("/api/artist", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<ArtistDto>(post);
        created.StageName.Should().Be(create.StageName);

        var get = await client.GetAsync($"/api/artist/{created.ArtistId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.Country = "Slovenia";
        var put = await client.PutAsJsonAsync($"/api/artist/{created.ArtistId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync<ArtistDto>(put)).Country.Should().Be("Slovenia");

        await AssertDeleteAsync(client, "/api/artist", created.ArtistId);
    }

    [Fact]
    public async Task Genre_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var create = ValidGenre(Unique("Integration Genre"));

        var post = await client.PostAsJsonAsync("/api/genre", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<GenreDto>(post);

        var get = await client.GetAsync($"/api/genre/{created.GenreId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.Description = "Updated integration-test description.";
        var put = await client.PutAsJsonAsync($"/api/genre/{created.GenreId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync<GenreDto>(put)).Description.Should().Be(create.Description);

        await AssertDeleteAsync(client, "/api/genre", created.GenreId);
    }

    [Fact]
    public async Task Album_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var artist = await FirstAsync<ArtistDto>(client, "/api/artist");
        var create = new AlbumCreateUpdateDto
        {
            Title = Unique("Integration Album"),
            ReleaseDate = new DateTime(2024, 1, 1),
            Label = "Test Label",
            TotalTracks = 8,
            Rating = 4.2,
            ArtistId = artist.ArtistId
        };

        var post = await client.PostAsJsonAsync("/api/album", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<AlbumDto>(post);
        created.Artist.ArtistId.Should().Be(artist.ArtistId);

        var get = await client.GetAsync($"/api/album/{created.AlbumId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.Rating = 4.8;
        var put = await client.PutAsJsonAsync($"/api/album/{created.AlbumId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync<AlbumDto>(put)).Rating.Should().Be(4.8);

        await AssertDeleteAsync(client, "/api/album", created.AlbumId);
    }

    [Fact]
    public async Task Song_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var artist = await FirstAsync<ArtistDto>(client, "/api/artist");
        var album = await FirstAsync<AlbumDto>(client, "/api/album");
        var genre = await FirstAsync<GenreDto>(client, "/api/genre");
        var create = new SongCreateUpdateDto
        {
            Title = Unique("Integration Song"),
            Duration = TimeSpan.FromMinutes(3),
            ReleaseDate = new DateTime(2024, 2, 1),
            PlayCount = 10,
            PopularityScore = 50,
            Mood = MoodType.Energetic,
            IsExplicit = false,
            ArtistId = artist.ArtistId,
            AlbumId = album.AlbumId,
            GenreId = genre.GenreId
        };

        var post = await client.PostAsJsonAsync("/api/song", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<SongDto>(post);

        var get = await client.GetAsync($"/api/song/{created.SongId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.PlayCount = 25;
        var put = await client.PutAsJsonAsync($"/api/song/{created.SongId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync<SongDto>(put)).PlayCount.Should().Be(25);

        await AssertDeleteAsync(client, "/api/song", created.SongId);
    }

    [Fact]
    public async Task User_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var create = new UserCreateUpdateDto
        {
            Username = Unique("integration-user"),
            Email = $"{Guid.NewGuid():N}@example.com",
            RegistrationDate = DateTime.UtcNow,
            FavoriteGenreName = "Rock",
            IsPremium = false
        };

        var post = await client.PostAsJsonAsync("/api/user", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<UserDto>(post);

        var get = await client.GetAsync($"/api/user/{created.UserId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.IsPremium = true;
        var put = await client.PutAsJsonAsync($"/api/user/{created.UserId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync<UserDto>(put)).IsPremium.Should().BeTrue();

        await AssertDeleteAsync(client, "/api/user", created.UserId);
    }

    [Fact]
    public async Task Playlist_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var owner = await FirstAsync<UserDto>(client, "/api/user");
        var song = await FirstAsync<SongDto>(client, "/api/song");
        var create = new PlaylistCreateUpdateDto
        {
            Name = Unique("Integration Playlist"),
            Description = "Created by an end-to-end integration test.",
            IsPublic = true,
            OwnerId = owner.UserId,
            SongIds = new List<int> { song.SongId }
        };

        var post = await client.PostAsJsonAsync("/api/playlist", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadAsync<PlaylistDto>(post);
        created.Owner.Should().NotBeNull();
        created.Songs.Should().ContainSingle(s => s.SongId == song.SongId);

        var get = await client.GetAsync($"/api/playlist/{created.PlaylistId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        create.Name = Unique("Updated Playlist");
        create.IsPublic = false;
        var put = await client.PutAsJsonAsync($"/api/playlist/{created.PlaylistId}", create);
        put.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsync<PlaylistDto>(put);
        updated.Name.Should().Be(create.Name);
        updated.IsPublic.Should().BeFalse();

        await AssertDeleteAsync(client, "/api/playlist", created.PlaylistId);
    }

    [Theory]
    [InlineData("/api/artist")]
    [InlineData("/api/album")]
    [InlineData("/api/genre")]
    [InlineData("/api/song")]
    [InlineData("/api/playlist")]
    [InlineData("/api/user")]
    public async Task Put_ReturnsNotFound_ForUnknownId(string route)
    {
        var client = _factory.CreateClient();
        object model = route switch
        {
            "/api/artist" => new ArtistCreateUpdateDto { StageName = "Missing", Country = "Croatia" },
            "/api/album" => new AlbumCreateUpdateDto { Title = "Missing", Label = "Test", ArtistId = 1 },
            "/api/genre" => ValidGenre("Missing"),
            "/api/song" => new SongCreateUpdateDto { Title = "Missing", ArtistId = 1, AlbumId = 1, GenreId = 1 },
            "/api/playlist" => new PlaylistCreateUpdateDto { Name = "Missing", Description = "Missing playlist", OwnerId = 1 },
            _ => new UserCreateUpdateDto { Username = "missing", Email = "missing@example.com" }
        };

        var response = await client.PutAsJsonAsync($"{route}/999999", model);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(ApiRoutes))]
    public async Task Delete_ReturnsNotFound_ForUnknownId(string route)
    {
        var response = await _factory.CreateClient().DeleteAsync($"{route}/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_ForInvalidDto()
    {
        var client = _factory.CreateClient();

        var artist = await client.PostAsJsonAsync("/api/artist", new ArtistCreateUpdateDto());
        var album = await client.PostAsJsonAsync("/api/album", new AlbumCreateUpdateDto());
        var genre = await client.PostAsJsonAsync("/api/genre", new GenreCreateUpdateDto());
        var song = await client.PostAsJsonAsync("/api/song", new SongCreateUpdateDto());
        var playlist = await client.PostAsJsonAsync("/api/playlist", new PlaylistCreateUpdateDto());
        var user = await client.PostAsJsonAsync("/api/user", new UserCreateUpdateDto());

        artist.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        album.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        genre.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        song.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        playlist.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        user.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static GenreCreateUpdateDto ValidGenre(string name)
        => new()
        {
            Name = name,
            Description = "Created by an integration test."
        };

    private static string Unique(string prefix)
        => $"{prefix} {Guid.NewGuid():N}";

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>();
        value.Should().NotBeNull();
        return value!;
    }

    private static async Task<T> FirstAsync<T>(HttpClient client, string route)
    {
        var values = await client.GetFromJsonAsync<List<T>>(route);
        values.Should().NotBeNullOrEmpty();
        return values![0];
    }

    private static async Task AssertDeleteAsync(HttpClient client, string route, int id)
    {
        var delete = await client.DeleteAsync($"{route}/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var verify = await client.GetAsync($"{route}/{id}");
        verify.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
