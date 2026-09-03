using Playlist;
using Playlist.MockRepositories;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Repositories;
using Microsoft.AspNetCore.Identity;
using Playlist.Models;
using Playlist.Logging;
using Playlist.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Azure App Service ima trajni HOME/data direktorij. Lokalno koristimo App_Data/wwwroot,
// pa uploadi, logovi i kljucevi za login prezive restart Azure instance.
var azureHome = Environment.GetEnvironmentVariable("HOME");
var isAzure = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
var dataRoot = builder.Configuration["Storage:RootPath"];
if (string.IsNullOrWhiteSpace(dataRoot))
{
    dataRoot = isAzure && !string.IsNullOrWhiteSpace(azureHome)
        ? Path.Combine(azureHome, "data", "MusicBar")
        : Path.Combine(builder.Environment.ContentRootPath, "App_Data");
}

var uploadsRoot = isAzure || !string.IsNullOrWhiteSpace(builder.Configuration["Storage:RootPath"])
    ? Path.Combine(dataRoot, "uploads")
    : Path.Combine(
        builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"),
        "uploads");
var logsRoot = isAzure ? Path.Combine(dataRoot, "logs") : Path.Combine(builder.Environment.ContentRootPath, "Logs");

builder.Logging.AddProvider(new FileLoggerProvider(logsRoot));
builder.Services.AddSingleton(new FileStorageOptions(uploadsRoot));
builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataRoot, "keys")))
    .SetApplicationName("MusicBar");

// Dvije baze/logicka konteksta koriste isti SQL connection string:
// MusicBarDbContext je katalog, a ApplicationDbContext je ASP.NET Identity login.
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var authBuilder = builder.Services.AddAuthentication();
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    // Google login se registrira samo kada su oba podatka konfigurirana.
    // Zato aplikacija i bez Google kljuceva i dalje normalno podrzava lokalni login.
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
builder.Services.AddScoped<AchievementService>();
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
builder.Services.AddHttpClient<IAiDjService, OpenAiDjService>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Startup priprema obje baze prije nego aplikacija pocne primati zahtjeve.
// Metode su idempotentne: mogu se ponoviti bez dupliciranja osnovnih podataka.
using (var scope = app.Services.CreateScope())
{
    var musicContext = scope.ServiceProvider.GetRequiredService<MusicBarDbContext>();
    var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    DbInitializer.Initialize(musicContext);
    EnsureGenreCatalog(musicContext);
    if (identityContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        identityContext.Database.Migrate();
    }

    await EnsureIdentityDataAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

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
app.MapGet("/health", async (MusicBarDbContext dbContext) =>
    await dbContext.Database.CanConnectAsync()
        ? Results.Ok(new { status = "healthy" })
        : Results.Problem(statusCode: 503, title: "Database unavailable"));

// Primjeri LINQ upita iz laboratorijskih vjezbi. Ispisuju se samo u konzolu
// i ne mijenjaju podatke koji se koriste u web-aplikaciji.
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

static async Task EnsureIdentityDataAsync(IServiceProvider serviceProvider)
{
    // Kreira demonstracijske role/racune samo ako jos ne postoje.
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

    const string managerEmail = "manager@musicbar.local";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    if (managerUser == null)
    {
        managerUser = new AppUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            EmailConfirmed = true,
            OIB = "44444444444",
            JMBG = "4444444444444"
        };

        var createManagerResult = await userManager.CreateAsync(managerUser, "Manager!12345");
        if (createManagerResult.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }
    }
    else if (!await userManager.IsInRoleAsync(managerUser, "Manager"))
    {
        await userManager.AddToRoleAsync(managerUser, "Manager");
    }

    await EnsureDomainUserAsync(
        serviceProvider,
        managerEmail,
        "MusicBar Manager");

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

static void EnsureGenreCatalog(MusicBarDbContext context)
{
    // Nadopunjuje padajuci izbornik standardnim zanrovima bez brisanja postojecih.
    var standardGenres = new[]
    {
        "Unknown", "Pop", "Rock", "Alternative Rock", "Indie", "Grunge", "Punk",
        "Hip Hop", "R&B", "Soul", "Funk", "Jazz", "Blues", "Country", "Folk",
        "Classical", "Electronic", "House", "Techno", "Trance", "Drum and Bass",
        "Reggae", "Latin", "Metal", "Heavy Metal", "Sludge Metal"
    };
    var existing = context.Genres.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
    var nextId = context.Genres.Any() ? context.Genres.Max(x => x.GenreId) + 1 : 1;

    foreach (var name in standardGenres.Where(name => !existing.Contains(name)))
    {
        context.Genres.Add(new Genre
        {
            GenreId = nextId++,
            Name = name,
            Description = name == "Unknown"
                ? "Use when a reliable genre is not available."
                : $"Music categorized as {name}."
        });
    }

    context.SaveChanges();
}

static async Task EnsureDomainUserAsync(
    IServiceProvider serviceProvider,
    string email,
    string username)
{
    // Identity korisnik sluzi za prijavu, a domenski User za playliste/history.
    // Email je veza izmedu ta dva modela.
    var musicContext = serviceProvider.GetRequiredService<MusicBarDbContext>();
    var domainUser = await musicContext.Users.FirstOrDefaultAsync(user => user.Email == email);
    if (domainUser != null)
    {
        return;
    }

    musicContext.Users.Add(new User
    {
        Username = username,
        Email = email,
        RegistrationDate = DateTime.UtcNow,
        FavoriteGenreName = "Not selected",
        IsPremium = false
    });
    await musicContext.SaveChangesAsync();
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
