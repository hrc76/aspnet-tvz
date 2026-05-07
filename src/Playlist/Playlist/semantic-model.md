# Semantic Model - MusicBar

## Overview

MusicBar is a music library and playlist application.  
The application manages songs, artists, albums, genres, users, playlists and listening history.

The database model is based on the original object model from Lab 1, adapted for Entity Framework Core.

---

## Entities / Tables

### Song

Represents a single music track.

Main properties:
- SongId - primary key
- Title (required, string, max 200)
- Duration (required, timespan)
- ReleaseDate (nullable, DateTime)
- PlayCount (int, default 0)
- PopularityScore (decimal 0-100)
- Mood (enum MoodType: Happy, Sad, Energetic, Calm, etc.)
- IsExplicit (bool, default false)
- ArtistId - foreign key (required)
- AlbumId - foreign key (required)
- GenreId - foreign key (required)
- CreatedAt (DateTime, auto-set on creation)
- UpdatedAt (DateTime, auto-update on modification)

Indexes:
- Clustered on SongId
- Index on ArtistId (for quick artist lookup)
- Index on GenreId (for filtering by genre)

Relationships:
- One Song belongs to one Artist
- One Song belongs to one Album
- One Song belongs to one Genre
- One Song can be part of many Playlists

---

### Artist

Represents a music artist or band.

Main properties:
- ArtistId - primary key
- StageName (required, string, max 150, unique)
- Country (nullable, string, max 100)
- DebutDate (nullable, DateTime)
- Biography (nullable, text)
- IsActive (bool, default true)
- CreatedAt (DateTime, auto-set on creation)

Indexes:
- Clustered on ArtistId
- Unique index on StageName
- Index on IsActive (for filtering active artists)

Relationships:
- One Artist can have many Albums
- One Artist can have many Songs

---

### Album

Represents a music album or record.

Main properties:
- AlbumId - primary key
- Title (required, string, max 200)
- ReleaseDate (nullable, DateTime)
- Label (nullable, string, max 150)
- TotalTracks (int)
- Rating (decimal 0-10, nullable)
- ArtistId - foreign key (required)
- CreatedAt (DateTime, auto-set on creation)

Indexes:
- Clustered on AlbumId
- Index on ArtistId (for quick artist albums lookup)

Relationships:
- One Album belongs to one Artist
- One Album can contain many Songs

---

### Genre

Represents a music genre.

Main properties:
- GenreId - primary key
- Name (required, string, max 100, unique)
- Description (nullable, text)
- CreatedAt (DateTime, auto-set on creation)

Indexes:
- Clustered on GenreId
- Unique index on Name

Relationships:
- One Genre can contain many Songs

---

### User

Represents an application user.

Main properties:
- UserId - primary key
- Username (required, string, max 100, unique)
- Email (required, string, max 255, unique)
- RegistrationDate (DateTime, auto-set on creation)
- FavoriteGenreId (nullable, int, foreign key to Genre)
  - Note: Denormalized for convenience; references Genre.GenreId
- IsPremium (bool, default false)
- LastLoginAt (nullable, DateTime)
- CreatedAt (DateTime, auto-set on creation)
- UpdatedAt (DateTime, auto-update on modification)

Indexes:
- Clustered on UserId
- Unique index on Username
- Unique index on Email
- Index on FavoriteGenreId

Relationships:
- One User can own many Playlists
- One User can have many ListeningHistory records

---

### Playlist

Represents a user-created playlist.

Main properties:
- PlaylistId - primary key
- Name (required, string, max 200)
- Description (nullable, text)
- CreatedAt (DateTime, auto-set on creation)
- UpdatedAt (DateTime, auto-update on modification)
- IsPublic (bool, default false)
- Likes (int, default 0)
- OwnerId - foreign key (required, references User)

Relationships:
- One Playlist belongs to one User
- One Playlist can contain many Songs through PlaylistSong join entity
- One Song can appear in many Playlists

**Many-to-many:** Realized through PlaylistSong join entity.

### PlaylistSong (Join Entity)

Represents the association between a Playlist and a Song.

Main properties:
- PlaylistSongId - primary key
- PlaylistId - foreign key (required)
- SongId - foreign key (required)
- Position (int, order of song in playlist)
- AddedAt (DateTime, when song was added to playlist)

Indexes:
- Clustered on PlaylistSongId
- Composite unique index on (PlaylistId, SongId)
- Index on PlaylistId

---

### ListeningHistory

Represents a record of a user listening to a song.

Main properties:
- ListeningHistoryId - primary key
- ListenedAt (DateTime, when the song was listened)
- Repeats (int, how many times repeated during this listening session)
- UserId - foreign key (required, references User)
- SongId - foreign key (required, references Song)
- CreatedAt (DateTime, auto-set on creation)

Relationships:
- One ListeningHistory record belongs to one User
- One ListeningHistory record belongs to one Song
- User → Song (indirect through ListeningHistory)

Indexes:
- Clustered on ListeningHistoryId
- Index on UserId (for quick user history lookup)
- Index on SongId
- Composite index on (UserId, ListenedAt) for analytics queries

---

## Relationship Summary

### One-to-many relationships

- Artist → Albums (cascade delete)
- Artist → Songs (cascade delete)
- Album → Songs (cascade delete)
- Genre → Songs (restrict delete - cannot delete genre with songs)
- User → Playlists (cascade delete)
- User → ListeningHistory (cascade delete)

### Many-to-many relationships

- Playlist ↔ Song (through PlaylistSong join entity, cascade delete on playlist deletion)

### Lookup / Reference

- User.FavoriteGenreId → Genre (nullable, restrict delete)
- MoodType: Enum stored as integer (Happy=1, Sad=2, Energetic=3, Calm=4, etc.)

## Delete Behavior Strategy

| Relationship | Delete Behavior | Reason |
|---|---|---|
| Artist → Songs/Albums | Cascade | Clean up all related content |
| Album → Songs | Cascade | Clean up all songs in album |
| Genre → Songs | Restrict | Prevent orphaned genre references |
| User → Playlists | Cascade | Clean up user's playlists |
| User → ListeningHistory | Cascade | Clean up listening history |
| Playlist → PlaylistSong | Cascade | Clean up playlist entries |
| User.FavoriteGenreId | Restrict | Prevent NULL forcing |

---

## EF Core Adaptation

The model was adapted for Entity Framework Core by:

- Adding primary key properties (SongId, ArtistId, etc.)
- Adding foreign key properties (ArtistId, AlbumId, GenreId, OwnerId, UserId, SongId)
- Using virtual navigation properties for lazy loading support
- Replacing List<T> with ICollection<T> for better EF Core compatibility
- Creating explicit join entity (PlaylistSong) for many-to-many relationships
- Adding audit fields (CreatedAt, UpdatedAt, AddedAt) for tracking changes
- Configuring relationships and delete behavior in MusicBarDbContext OnModelCreating()
- Setting up cascade delete, restrict delete rules
- Defining unique indexes (StageName, Email, Username, Genre.Name)
- Creating composite indexes for common query patterns
- Setting up column constraints (max string lengths, nullable fields)
- Creating DbSet<T> properties for all main entities including PlaylistSong
---

## Best Practices & Design Decisions

### Data Integrity
- **Unique constraints** on natural identifiers (Username, Email, StageName, Genre.Name) to ensure data uniqueness
- **Foreign key constraints** enforce referential integrity
- **Cascade delete** used for ownership hierarchies (User → Playlists, Artist → Songs)
- **Restrict delete** on reference data (Genre) to prevent orphaned songs

### Performance Considerations
- **Indexes on foreign keys** speed up JOIN operations and filtering
- **Composite indexes** (UserId, ListenedAt) optimize common analytics queries
- **PlaylistSong join entity** includes Position for efficient ordering without re-sorting

### Audit Trail
- **CreatedAt** on all main entities for tracking creation time
- **UpdatedAt** on mutable entities (User, Playlist, Song, Album) for tracking modifications
- **ListeningHistory** serves as audit trail for user activity

### Scalability Notes
- **PlayCount** on Song could be denormalized (updated periodically from ListeningHistory) for performance
- **Likes** on Playlist could be moved to separate PlaylistLike entity for massive scale
- **FavoriteGenreId** denormalization trades normalization for faster user profile queries