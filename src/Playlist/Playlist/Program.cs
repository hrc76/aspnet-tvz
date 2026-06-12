using Playlist;
using Playlist.MockRepositories;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Repositories;
using Microsoft.AspNetCore.Identity;
using Playlist.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MusicBarDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MusicBarDbContext")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MusicBarDbContext")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var authBuilder = builder.Services.AddAuthentication();
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

builder.Services.AddScoped<SongRepository>();
builder.Services.AddScoped<ArtistRepository>();
builder.Services.AddScoped<AlbumRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<PlaylistRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ListeningHistoryRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SongMockRepository>();
builder.Services.AddSingleton<ArtistMockRepository>();
builder.Services.AddSingleton<AlbumMockRepository>();
builder.Services.AddSingleton<GenreMockRepository>();
builder.Services.AddSingleton<PlaylistMockRepository>();
builder.Services.AddSingleton<UserMockRepository>();
builder.Services.AddSingleton<ListeningHistoryMockRepository>();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FavoriteSongRepository>();
builder.Services.AddScoped<SavedAlbumRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var musicContext = scope.ServiceProvider.GetRequiredService<MusicBarDbContext>();
    var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    DbInitializer.Initialize(musicContext);
    if (identityContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        identityContext.Database.Migrate();
    }

    await EnsureIdentityDataAsync(scope.ServiceProvider, app.Environment.IsDevelopment());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "library",
    pattern: "library",
    defaults: new { controller = "Library", action = "Index" });

app.MapControllerRoute(
    name: "discover",
    pattern: "discover",
    defaults: new { controller = "Discover", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.MapRazorPages();

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

static async Task EnsureRoleAsync(IServiceProvider services, string roleName)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

static async Task EnsureIdentityDataAsync(IServiceProvider serviceProvider, bool seedDemoUsers)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
    await EnsureRoleAsync(serviceProvider, "Admin");
    await EnsureRoleAsync(serviceProvider, "Manager");

    const string adminEmail = "admin@musicbar.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            OIB = "11111111111",
            JMBG = "1111111111111"
        };

        var createResult = await userManager.CreateAsync(adminUser, "Admin!12345");
        if (createResult.Succeeded)
        {
            await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Manager" });
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Manager"))
        {
            await userManager.AddToRoleAsync(adminUser, "Manager");
        }
    }

    if (seedDemoUsers)
    {
        await EnsureDemoIdentityUserAsync(
            userManager,
            "hrc@gmail.com",
            "22222222222",
            "2222222222222");

        await EnsureDemoIdentityUserAsync(
            userManager,
            "jurs@gmail.com",
            "33333333333",
            "3333333333333");
    }
}

static async Task EnsureDemoIdentityUserAsync(
    UserManager<AppUser> userManager,
    string email,
    string oib,
    string jmbg)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            OIB = oib,
            JMBG = jmbg
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return;
        }
    }

    // Demo-only password: stored as an Identity hash, never as plain text.
    user.UserName = email;
    user.Email = email;
    user.OIB = oib;
    user.JMBG = jmbg;
    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "password");
    user.SecurityStamp = Guid.NewGuid().ToString();
    user.EmailConfirmed = true;
    await userManager.UpdateAsync(user);
}
