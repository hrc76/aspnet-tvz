using Playlist.Models;
using Microsoft.EntityFrameworkCore;

namespace Playlist.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MusicBarDbContext context)
        {
            context.Database.Migrate();

            if (context.Songs.Any())
            {
                return;
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