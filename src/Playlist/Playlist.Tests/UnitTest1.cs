using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Playlist.ViewModels.Api;
using Xunit;

namespace Playlist.Tests;

public class PlaylistApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PlaylistApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_Playlists_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/playlist");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Response body: {0}", body);
        var playlists = await response.Content.ReadFromJsonAsync<List<PlaylistDto>>();
        playlists.Should().NotBeNull();
        playlists.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForUnknownPlaylist()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/playlist/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPutDelete_Playlist_CRUD_Works()
    {
        var client = _factory.CreateClient();

        var usersResponse = await client.GetAsync("/api/user");
        usersResponse.EnsureSuccessStatusCode();
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Should().NotBeEmpty();

        var ownerId = users[0].UserId;

        var createModel = new PlaylistCreateUpdateDto
        {
            Name = "Test API Playlist",
            Description = "Created by integration test",
            IsPublic = true,
            OwnerId = ownerId,
            SongIds = new List<int> { 1 }
        };

        var postResponse = await client.PostAsJsonAsync("/api/playlist", createModel);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlaylist = await postResponse.Content.ReadFromJsonAsync<PlaylistDto>();
        createdPlaylist.Should().NotBeNull();
        createdPlaylist!.Name.Should().Be(createModel.Name);
        createdPlaylist.Owner.Should().NotBeNull();
        createdPlaylist.Owner!.UserId.Should().Be(ownerId);

        var updateModel = new PlaylistCreateUpdateDto
        {
            Name = "Updated API Playlist",
            Description = createdPlaylist.Description,
            IsPublic = false,
            OwnerId = ownerId,
            SongIds = createModel.SongIds
        };

        var putResponse = await client.PutAsJsonAsync($"/api/playlist/{createdPlaylist.PlaylistId}", updateModel);
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPlaylist = await putResponse.Content.ReadFromJsonAsync<PlaylistDto>();
        updatedPlaylist.Should().NotBeNull();
        updatedPlaylist!.Name.Should().Be(updateModel.Name);
        updatedPlaylist.IsPublic.Should().BeFalse();

        var deleteResponse = await client.DeleteAsync($"/api/playlist/{createdPlaylist.PlaylistId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var verifyResponse = await client.GetAsync($"/api/playlist/{createdPlaylist.PlaylistId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
