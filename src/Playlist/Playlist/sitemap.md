# Sitemap - MusicBar

This document describes the available URLs in the MusicBar ASP.NET Core MVC application.

---

## Main navigation routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/` | HomeController | Index | Views/Home/Index.cshtml |
| `/Home/Index` | HomeController | Index | Views/Home/Index.cshtml |
| `/library` | LibraryController | Index | Views/Library/Index.cshtml |
| `/discover` | DiscoverController | Index | Views/Discover/Index.cshtml |

---

## Custom routes

| URL | Controller | Action | View | Description |
|---|---|---|---|---|
| `/library` | LibraryController | Index | Views/Library/Index.cshtml | User library page |
| `/discover` | DiscoverController | Index | Views/Discover/Index.cshtml | Discover and search page |
| `/listen/{id}` | SongController | Details | Views/Song/Details.cshtml | Custom route for song details |
| `/record/{id}` | AlbumController | Details | Views/Album/Details.cshtml | Custom route for album details |

---

## Song routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Song` | SongController | Index | Views/Song/Index.cshtml |
| `/Song/Index` | SongController | Index | Views/Song/Index.cshtml |
| `/Song/Details/{id}` | SongController | Details | Views/Song/Details.cshtml |

---

## Artist routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Artist` | ArtistController | Index | Views/Artist/Index.cshtml |
| `/Artist/Index` | ArtistController | Index | Views/Artist/Index.cshtml |
| `/Artist/Details/{id}` | ArtistController | Details | Views/Artist/Details.cshtml |

---

## Album routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Album` | AlbumController | Index | Views/Album/Index.cshtml |
| `/Album/Index` | AlbumController | Index | Views/Album/Index.cshtml |
| `/Album/Details/{id}` | AlbumController | Details | Views/Album/Details.cshtml |

---

## Genre routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Genre` | GenreController | Index | Views/Genre/Index.cshtml |
| `/Genre/Index` | GenreController | Index | Views/Genre/Index.cshtml |
| `/Genre/Details/{id}` | GenreController | Details | Views/Genre/Details.cshtml |

---

## Playlist routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Playlist` | PlaylistController | Index | Views/Playlist/Index.cshtml |
| `/Playlist/Index` | PlaylistController | Index | Views/Playlist/Index.cshtml |
| `/Playlist/Details/{id}` | PlaylistController | Details | Views/Playlist/Details.cshtml |

---

## User routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/User` | UserController | Index | Views/User/Index.cshtml |
| `/User/Index` | UserController | Index | Views/User/Index.cshtml |
| `/User/Details/{id}` | UserController | Details | Views/User/Details.cshtml |

---

## Listening History routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/ListeningHistory` | ListeningHistoryController | Index | Views/ListeningHistory/Index.cshtml |
| `/ListeningHistory/Index` | ListeningHistoryController | Index | Views/ListeningHistory/Index.cshtml |
| `/ListeningHistory/Details/{id}` | ListeningHistoryController | Details | Views/ListeningHistory/Details.cshtml |

---

## Utility routes

| URL | Controller | Action | View |
|---|---|---|---|
| `/Home/Privacy` | HomeController | Privacy | Views/Home/Privacy.cshtml |
| `/Home/Error` | HomeController | Error | Views/Shared/Error.cshtml |

---

## Routing explanation

The application uses the default MVC route:

csharp
{controller=Home}/{action=Index}/{id?}