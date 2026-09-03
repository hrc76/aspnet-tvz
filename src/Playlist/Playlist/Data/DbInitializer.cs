using Playlist.Models;
using Microsoft.EntityFrameworkCore;

namespace Playlist.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MusicBarDbContext context)
        {
            if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                context.Database.Migrate();
            }

            // Prazna ili nepotpuna demo baza ponovno dobiva cijeli osnovni katalog.
            bool needsReseed = !context.Songs.Any() || context.Songs.Count() < 30;

            if (needsReseed && context.Songs.Any())
            {
                // Brisanje ide redoslijedom koji ne krsi foreign key veze.
                if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                {
                    context.Database.ExecuteSqlRaw("DELETE FROM [PlaylistSong]");
                }

                context.ListeningHistories.RemoveRange(context.ListeningHistories);
                context.FavoriteSongs.RemoveRange(context.FavoriteSongs);
                context.SavedAlbums.RemoveRange(context.SavedAlbums);
                context.Playlists.RemoveRange(context.Playlists);
                context.Songs.RemoveRange(context.Songs);
                context.Albums.RemoveRange(context.Albums);
                context.Genres.RemoveRange(context.Genres);
                context.Artists.RemoveRange(context.Artists);
                context.Users.RemoveRange(context.Users);
                context.SaveChanges();
            }

            if (needsReseed)
            {
                var data = DataSeeder.Seed();
                context.Genres.AddRange(data.Genres);
                context.Artists.AddRange(data.Artists);
                context.Albums.AddRange(data.Albums);
                context.Songs.AddRange(data.Songs);
                context.Users.AddRange(data.Users);
                context.Playlists.AddRange(data.Playlists);
                context.ListeningHistories.AddRange(data.ListeningHistories);
                context.SaveChanges();
            }

            // Demo analytics treba konkretne podatke i na vec postojecoj Azure bazi.
            EnsureDemoListeningHistory(context);
        }

        private static void EnsureDemoListeningHistory(MusicBarDbContext context)
        {
            var profiles = new[]
            {
                new { Email = "hrc@gmail.com", Songs = new[] { 1, 3, 16, 17, 7, 8, 2, 19, 20, 9, 18, 21, 3, 16, 1, 7 } },
                new { Email = "jurs@gmail.com", Songs = new[] { 4, 13, 10, 22, 23, 6, 14, 28, 11, 24, 15, 29, 4, 13, 25, 30 } }
            };
            var nextId = context.ListeningHistories.Any()
                ? context.ListeningHistories.Max(item => item.ListeningHistoryId) + 1
                : 1;

            foreach (var profile in profiles)
            {
                var user = context.Users.FirstOrDefault(item => item.Email == profile.Email);
                if (user == null) continue;
                var existingCount = context.ListeningHistories.Count(item => item.UserId == user.UserId);
                for (var index = existingCount; index < profile.Songs.Length; index++)
                {
                    if (!context.Songs.Any(song => song.SongId == profile.Songs[index])) continue;
                    context.ListeningHistories.Add(new ListeningHistory
                    {
                        ListeningHistoryId = nextId++,
                        UserId = user.UserId,
                        SongId = profile.Songs[index],
                        Repeats = index % 4 == 0 ? 3 : index % 3 == 0 ? 2 : 1,
                        ListenedAt = DateTime.UtcNow.AddHours(-(profile.Songs.Length - index) * 7)
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
