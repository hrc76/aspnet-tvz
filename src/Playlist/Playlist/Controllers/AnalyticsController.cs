using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;
using Playlist.Repositories;
using Playlist.ViewModels;

namespace Playlist.Controllers;

[Authorize]
public sealed class AnalyticsController : Controller
{
    private readonly MusicBarDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly UserRepository _userRepository;

    public AnalyticsController(MusicBarDbContext context, UserManager<AppUser> userManager, UserRepository userRepository)
    {
        _context = context;
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var appUser = await _userManager.GetUserAsync(User);
        var domainUser = string.IsNullOrWhiteSpace(appUser?.Email) ? null : _userRepository.GetByEmail(appUser.Email);
        if (domainUser == null) return View(new ListeningAnalyticsViewModel());

        // Obican korisnik vidi samo sebe; admin i manager dobivaju globalni dashboard.
        var showAllUsers = User.IsInRole("Admin") || User.IsInRole("Manager");
        var historyQuery = _context.ListeningHistories.AsNoTracking()
            .Include(item => item.Song).ThenInclude(song => song.Artist)
            .Include(item => item.Song).ThenInclude(song => song.Album)
            .Include(item => item.Song).ThenInclude(song => song.Genre)
            .AsQueryable();

        if (!showAllUsers)
        {
            historyQuery = historyQuery.Where(item => item.UserId == domainUser.UserId);
        }

        var history = await historyQuery.OrderByDescending(item => item.ListenedAt)
            .ToListAsync(cancellationToken);

        var model = BuildAnalytics(history);
        model.IsGlobal = showAllUsers;
        model.ProfileCount = showAllUsers ? history.Select(item => item.UserId).Distinct().Count() : 1;
        return View(model);
    }

    internal static ListeningAnalyticsViewModel BuildAnalytics(IReadOnlyCollection<ListeningHistory> history)
    {
        // Sirove zapise pretvaramo u brojeve i rang-liste koje Razor view samo prikazuje.
        // Time statisticka logika ostaje izvan HTML-a i moze se zasebno testirati.
        var totalPlays = history.Sum(item => Math.Max(1, item.Repeats));
        var today = DateTime.UtcNow.Date;
        var daily = Enumerable.Range(0, 7).Select(offset => today.AddDays(offset - 6)).ToList();
        var dayValues = daily.Select(day => new
        {
            Day = day,
            Plays = history.Where(item => item.ListenedAt.ToUniversalTime().Date == day)
                .Sum(item => Math.Max(1, item.Repeats))
        }).ToList();
        var maxDay = Math.Max(1, dayValues.Max(item => item.Plays));

        var activeDays = history.Select(item => item.ListenedAt.ToUniversalTime().Date).Distinct().OrderByDescending(day => day).ToList();
        var streak = 0;
        if (activeDays.Count > 0)
        {
            var cursor = activeDays[0] == today || activeDays[0] == today.AddDays(-1) ? activeDays[0] : DateTime.MinValue;
            while (cursor != DateTime.MinValue && activeDays.Contains(cursor)) { streak++; cursor = cursor.AddDays(-1); }
        }

        return new ListeningAnalyticsViewModel
        {
            TotalPlays = totalPlays,
            UniqueSongs = history.Select(item => item.SongId).Distinct().Count(),
            ListeningMinutes = (int)Math.Round(history.Sum(item => item.Song.Duration.TotalMinutes * Math.Max(1, item.Repeats))),
            ListeningStreak = streak,
            PeakListeningTime = history.Count == 0 ? "No data" : DescribeHour(history.GroupBy(item => item.ListenedAt.Hour)
                .OrderByDescending(group => group.Sum(item => Math.Max(1, item.Repeats))).First().Key),
            TopSongs = history.GroupBy(item => item.SongId).Select(group => new AnalyticsRankItem
                {
                    Name = group.First().Song.Title,
                    Subtitle = group.First().Song.Artist.StageName,
                    ImageUrl = group.First().Song.Album.CoverUrl,
                    Plays = group.Sum(item => Math.Max(1, item.Repeats))
                }).OrderByDescending(item => item.Plays).ThenBy(item => item.Name).Take(5).ToList(),
            TopArtists = history.GroupBy(item => item.Song.ArtistId).Select(group => new AnalyticsRankItem
                {
                    Name = group.First().Song.Artist.StageName,
                    Subtitle = $"{group.Select(item => item.SongId).Distinct().Count()} tracks",
                    Plays = group.Sum(item => Math.Max(1, item.Repeats))
                }).OrderByDescending(item => item.Plays).ThenBy(item => item.Name).Take(5).ToList(),
            Genres = BuildBars(history.GroupBy(item => item.Song.Genre.Name)
                .Select(group => (group.Key, group.Sum(item => Math.Max(1, item.Repeats)))), totalPlays),
            Moods = BuildBars(history.GroupBy(item => item.Song.Mood.ToString())
                .Select(group => (group.Key, group.Sum(item => Math.Max(1, item.Repeats)))), totalPlays),
            LastSevenDays = dayValues.Select(item => new AnalyticsDayItem
                {
                    Label = item.Day.ToString("ddd").ToUpperInvariant(),
                    Plays = item.Plays,
                    Percentage = (int)Math.Round(item.Plays * 100d / maxDay)
                }).ToList()
        };
    }

    private static List<AnalyticsBarItem> BuildBars(IEnumerable<(string Name, int Plays)> values, int total) => values
        .OrderByDescending(item => item.Plays).Select(item => new AnalyticsBarItem
        {
            Name = item.Name,
            Plays = item.Plays,
            Percentage = total == 0 ? 0 : (int)Math.Round(item.Plays * 100d / total)
        }).ToList();

    private static string DescribeHour(int hour) => hour switch
    {
        >= 5 and < 12 => "Morning",
        >= 12 and < 17 => "Afternoon",
        >= 17 and < 22 => "Evening",
        _ => "Late night"
    };
}
