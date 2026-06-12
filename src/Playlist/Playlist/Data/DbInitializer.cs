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

            // Re-seed if the DB is empty OR has fewer songs than the current seed (30)
            bool needsReseed = !context.Songs.Any() || context.Songs.Count() < 30;

            if (!needsReseed) return;

            if (context.Songs.Any())
            {
                // Clear all data in FK-safe order before re-seeding
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
    }
}
