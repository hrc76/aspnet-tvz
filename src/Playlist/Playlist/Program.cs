using Playlist;
using Playlist.MockRepositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SongMockRepository>();
builder.Services.AddSingleton<ArtistMockRepository>();
builder.Services.AddSingleton<AlbumMockRepository>();
builder.Services.AddSingleton<GenreMockRepository>();
builder.Services.AddSingleton<PlaylistMockRepository>();
builder.Services.AddSingleton<UserMockRepository>();
builder.Services.AddSingleton<ListeningHistoryMockRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    
var data = DataSeeder.Seed();

var allSongs = data.Songs;
var allArtists = data.Artists;
var allPlaylists = data.Playlists;
var allUsers = data.Users;

var top3Songs = allSongs
    .OrderByDescending(s => s.PlayCount)
    .Take(3)
    .ToList();

Console.WriteLine("Top 3 pjesme");
foreach (var s in top3Songs)
{
    Console.WriteLine($"{s.Title} - {s.Artist.StageName} ({s.PlayCount} plays)");
}
Console.WriteLine();

var technoSongs = allSongs
    .Where(s => s.Genre.Name == "Techno")
    .ToList();

Console.WriteLine("Sve tehno pjesme");
foreach (var s in technoSongs)
{
    Console.WriteLine($"{s.Title} - {s.Artist.StageName}");
}
Console.WriteLine();

var longSongs = allSongs
    .Where(s => s.Duration > new TimeSpan(0, 5, 0))
    .ToList();

Console.WriteLine("Pjesme duze od 5 minuta");
foreach (var s in longSongs)
{
    Console.WriteLine($"{s.Title} - Duration: {s.Duration}");
}
Console.WriteLine();

var topArtists = allArtists
    .Where(a => a.Albums.Any(al => al.Rating > 4.7))
    .ToList();

Console.WriteLine("Izvodaci s albumom ocijenjenim iznad 4.7");
foreach (var a in topArtists)
{
    Console.WriteLine($"{a.StageName}");
}
Console.WriteLine();

var publicPlaylists = allPlaylists
    .Where(p => p.IsPublic)
    .ToList();

Console.WriteLine("Javne playliste");
foreach (var p in publicPlaylists)
{
    Console.WriteLine($"{p.Name} - Owner: {p.Owner.Username}");
}
Console.WriteLine();

var premiumUsers = allUsers
    .Where(u => u.IsPremium)
    .ToList();

Console.WriteLine("Premium korisnici");
foreach (var u in premiumUsers)
{
    Console.WriteLine($"{u.Username} - Favorite genre: {u.FavoriteGenreName}");
}
Console.WriteLine();

var songsByGenre = allSongs
    .GroupBy(s => s.Genre.Name)
    .Select(g => new
    {
        Genre = g.Key,
        Count = g.Count()
    })
    .ToList();
    
Console.WriteLine("Broj pjesama po zanru");

foreach (var g in songsByGenre)
{
    Console.WriteLine($"{g.Genre}: {g.Count}");
}
Console.WriteLine();

var similarSongs = allSongs
    .Where(s => s.SongId != 1 && s.Genre.Name == "Sludge Metal")
    .ToList();

Console.WriteLine("Slicne pjesme (Sludge Metal zanra)");
foreach (var s in similarSongs)
{
    Console.WriteLine($"{s.Title} - {s.Artist.StageName}");
}
Console.WriteLine();

app.Run();