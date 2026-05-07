using Microsoft.EntityFrameworkCore;
using Playlist.Models;

namespace Playlist.Data
{
    public class MusicBarDbContext : DbContext
    {
        public MusicBarDbContext(DbContextOptions<MusicBarDbContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs => Set<Song>();
        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Album> Albums => Set<Album>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Playlist.Models.Playlist> Playlists => Set<Playlist.Models.Playlist>();
        public DbSet<ListeningHistory> ListeningHistories => Set<ListeningHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys to not auto-generate (use manual IDs from DataSeeder)
            modelBuilder.Entity<Song>()
                .Property(s => s.SongId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Artist>()
                .Property(a => a.ArtistId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Album>()
                .Property(a => a.AlbumId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Genre>()
                .Property(g => g.GenreId)
                .ValueGeneratedNever();

            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Playlist.Models.Playlist>()
                .Property(p => p.PlaylistId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ListeningHistory>()
                .Property(l => l.ListeningHistoryId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Artist)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Album)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Playlist.Models.Playlist>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListeningHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.ListeningHistory)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListeningHistory>()
                .HasOne(h => h.Song)
                .WithMany()
                .HasForeignKey(h => h.SongId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}