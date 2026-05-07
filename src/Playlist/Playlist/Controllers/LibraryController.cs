using Microsoft.AspNetCore.Mvc;
using Playlist.MockRepositories;
using Playlist.ViewModels;

namespace Playlist.Controllers
{
    public class LibraryController : Controller
    {
        private readonly SongMockRepository _songRepository;
        private readonly AlbumMockRepository _albumRepository;
        private readonly PlaylistMockRepository _playlistRepository;

        public LibraryController(
            SongMockRepository songRepository,
            AlbumMockRepository albumRepository,
            PlaylistMockRepository playlistRepository)
        {
            _songRepository = songRepository;
            _albumRepository = albumRepository;
            _playlistRepository = playlistRepository;
        }

        public IActionResult Index()
        {
            var favoriteSongs = _songRepository.GetAll()
                .OrderByDescending(s => s.PlayCount)
                .Take(4)
                .ToList();

            var albumItems = _albumRepository.GetAll()
                .Select(a => new LibraryItemViewModel
                {
                    Id = a.AlbumId,
                    Type = "Album",
                    Title = a.Title,
                    Subtitle = a.Artist.StageName,
                    Meta = $"{a.ReleaseDate.Year} · {a.Rating} ★",
                    ImagePath = $"/images/albums/{a.Title.Replace(" ", "")}.jpg",
                    ControllerName = "Album"
                });

            var playlistItems = _playlistRepository.GetAll()
                .Select(p => new LibraryItemViewModel
                {
                    Id = p.PlaylistId,
                    Type = "Playlist",
                    Title = p.Name,
                    Subtitle = p.Owner.Username,
                    Meta = $"{p.Songs.Count} songs · {(p.IsPublic ? "Public" : "Private")}",
                    ImagePath = $"/images/playlists/{p.Name.Replace(" ", "")}.jpg",
                    ControllerName = "Playlist"
                });

            var model = new LibraryViewModel
            {
                FavoriteSongs = favoriteSongs,
                LibraryItems = albumItems
                    .Concat(playlistItems)
                    .OrderBy(i => i.Title)
                    .ToList()
            };

            return View(model);
        }
    }
}