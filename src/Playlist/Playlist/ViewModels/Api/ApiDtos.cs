using Playlist.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Playlist.ViewModels.Api
{
    public class ArtistSummaryDto
    {
        public int ArtistId { get; set; }
        public string StageName { get; set; } = string.Empty;
    }

    public class AlbumSummaryDto
    {
        public int AlbumId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class GenreSummaryDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SongSummaryDto
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
    }

    public class PlaylistAttachmentDto
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class ArtistDto : ArtistSummaryDto
    {
        public string Country { get; set; } = string.Empty;
        public DateTime DebutDate { get; set; }
        public string Biography { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ArtistCreateUpdateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string StageName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public DateTime DebutDate { get; set; }

        [StringLength(2000)]
        public string Biography { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class GenreDto : GenreSummaryDto
    {
        public string Description { get; set; } = string.Empty;
    }

    public class GenreCreateUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
    }

    public class AlbumDto : AlbumSummaryDto
    {
        public DateTime ReleaseDate { get; set; }
        public string Label { get; set; } = string.Empty;
        public int TotalTracks { get; set; }
        public double Rating { get; set; }
        public string? CoverUrl { get; set; }
        public ArtistSummaryDto Artist { get; set; } = new ArtistSummaryDto();
    }

    public class AlbumCreateUpdateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Label { get; set; } = string.Empty;

        [Range(0, 500)]
        public int TotalTracks { get; set; }

        [Range(0.0, 5.0)]
        public double Rating { get; set; }

        public string? CoverUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int ArtistId { get; set; }
    }

    public class SongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int PlayCount { get; set; }
        public double PopularityScore { get; set; }
        public MoodType Mood { get; set; }
        public bool IsExplicit { get; set; }
        public string? AudioUrl { get; set; }
        public ArtistSummaryDto ArtistSummary { get; set; } = new ArtistSummaryDto();
        public AlbumSummaryDto Album { get; set; } = new AlbumSummaryDto();
        public GenreSummaryDto GenreSummary { get; set; } = new GenreSummaryDto();
    }

    public class SongCreateUpdateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }
        public DateTime ReleaseDate { get; set; }

        [Range(0, int.MaxValue)]
        public int PlayCount { get; set; }

        [Range(0.0, 100.0)]
        public double PopularityScore { get; set; }

        public MoodType Mood { get; set; }
        public bool IsExplicit { get; set; }
        public string? AudioUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int ArtistId { get; set; }

        [Range(1, int.MaxValue)]
        public int AlbumId { get; set; }

        [Range(1, int.MaxValue)]
        public int GenreId { get; set; }
    }

    public class PlaylistDto
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public int Likes { get; set; }
        public UserSummaryDto? Owner { get; set; }
        public List<SongSummaryDto> Songs { get; set; } = new List<SongSummaryDto>();
        public List<PlaylistAttachmentDto> Attachments { get; set; } = new List<PlaylistAttachmentDto>();
    }

    public class PlaylistCreateUpdateDto
    {
        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        public bool IsPublic { get; set; }

        [Range(1, int.MaxValue)]
        public int? OwnerId { get; set; }

        public List<int> SongIds { get; set; } = new List<int>();
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string FavoriteGenreName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public int PlaylistCount { get; set; }
    }

    public class UserCreateUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        [StringLength(100)]
        public string FavoriteGenreName { get; set; } = string.Empty;

        public bool IsPremium { get; set; }
    }

    public static class ApiMappingExtensions
    {
        public static ArtistSummaryDto ToSummaryDto(this Artist artist)
            => new()
            {
                ArtistId = artist.ArtistId,
                StageName = artist.StageName
            };

        public static ArtistDto ToDto(this Artist artist)
            => new()
            {
                ArtistId = artist.ArtistId,
                StageName = artist.StageName,
                Country = artist.Country,
                DebutDate = artist.DebutDate,
                Biography = artist.Biography,
                IsActive = artist.IsActive
            };

        public static GenreDto ToDto(this Genre genre)
            => new()
            {
                GenreId = genre.GenreId,
                Name = genre.Name,
                Description = genre.Description
            };

        public static GenreSummaryDto ToSummaryDto(this Genre genre)
            => new()
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            };

        public static AlbumDto ToDto(this Album album)
            => new()
            {
                AlbumId = album.AlbumId,
                Title = album.Title,
                ReleaseDate = album.ReleaseDate,
                Label = album.Label,
                TotalTracks = album.TotalTracks,
                Rating = album.Rating,
                CoverUrl = album.CoverUrl,
                Artist = album.Artist?.ToSummaryDto() ?? new ArtistSummaryDto()
            };

        public static AlbumSummaryDto ToSummaryDto(this Album album)
            => new()
            {
                AlbumId = album.AlbumId,
                Title = album.Title
            };

        public static SongDto ToDto(this Song song)
            => new()
            {
                SongId = song.SongId,
                Title = song.Title,
                Artist = song.Artist?.StageName ?? string.Empty,
                Genre = song.Genre?.Name ?? string.Empty,
                Duration = song.Duration,
                ReleaseDate = song.ReleaseDate,
                PlayCount = song.PlayCount,
                PopularityScore = song.PopularityScore,
                Mood = song.Mood,
                IsExplicit = song.IsExplicit,
                AudioUrl = song.AudioUrl,
                ArtistSummary = song.Artist?.ToSummaryDto() ?? new ArtistSummaryDto(),
                Album = song.Album?.ToSummaryDto() ?? new AlbumSummaryDto(),
                GenreSummary = song.Genre?.ToSummaryDto() ?? new GenreSummaryDto()
            };

        public static PlaylistAttachmentDto ToDto(this PlaylistAttachment attachment)
            => new()
            {
                Id = attachment.ID,
                PlaylistId = attachment.PlaylistId,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                CreatedAt = attachment.CreatedAt
            };

        public static PlaylistDto ToDto(this Playlist.Models.Playlist playlist)
            => new()
            {
                PlaylistId = playlist.PlaylistId,
                Name = playlist.Name,
                Description = playlist.Description,
                CreatedAt = playlist.CreatedAt,
                IsPublic = playlist.IsPublic,
                Likes = playlist.Likes,
                Owner = playlist.Owner == null ? null : new UserSummaryDto
                {
                    UserId = playlist.Owner.UserId,
                    Username = playlist.Owner.Username
                },
                Songs = playlist.Songs?.Select(s => new SongSummaryDto
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    Artist = s.Artist?.StageName ?? string.Empty,
                    Genre = s.Genre?.Name ?? string.Empty
                }).ToList() ?? new List<SongSummaryDto>(),
                Attachments = playlist.Attachments?.Select(a => a.ToDto()).ToList() ?? new List<PlaylistAttachmentDto>()
            };

        public static UserDto ToDto(this User user)
            => new()
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                FavoriteGenreName = user.FavoriteGenreName,
                IsPremium = user.IsPremium,
                PlaylistCount = user.Playlists?.Count ?? 0
            };
    }
}
