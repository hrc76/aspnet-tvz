using Playlist.Models;

namespace Playlist
{
    public static class DataSeeder
    {
        public static (
        List<Song> Songs,
        List<Artist> Artists,
        List<Album> Albums,
        List<Genre> Genres,
        List<Models.Playlist> Playlists,
        List<User> Users,
        List<ListeningHistory> ListeningHistories
             ) Seed()
        {
            //  GENRES 
            var sludge = new Genre
            {
                GenreId = 1,
                Name = "Sludge Metal",
                Description = "Heavy, slow and aggressive metal genre"
            };

            var hipHop = new Genre
            {
                GenreId = 2,
                Name = "Hip Hop",
                Description = "Rhythmic music with rap vocals and beats"
            };

            var techno = new Genre
            {
                GenreId = 3,
                Name = "Techno",
                Description = "Electronic dance music with repetitive beats"
            };

            //  ARTISTS 
            var crowbar = new Artist
            {
                ArtistId = 1,
                StageName = "Crowbar",
                Country = "USA",
                DebutDate = new DateTime(1990, 1, 1),
                Biography = "American sludge metal band from New Orleans.",
                IsActive = true
            };

            var eyehategod = new Artist
            {
                ArtistId = 2,
                StageName = "Eyehategod",
                Country = "USA",
                DebutDate = new DateTime(1988, 1, 1),
                Biography = "Influential sludge metal band.",
                IsActive = true
            };

            var acidBath = new Artist
            {
                ArtistId = 3,
                StageName = "Acid Bath",
                Country = "USA",
                DebutDate = new DateTime(1991, 1, 1),
                Biography = "Cult sludge metal band with dark and unique sound.",
                IsActive = false
            };

            var nas = new Artist
            {
                ArtistId = 4,
                StageName = "Nas",
                Country = "USA",
                DebutDate = new DateTime(1991, 1, 1),
                Biography = "Legendary hip hop artist from Queensbridge.",
                IsActive = true
            };

            var funkdoobiest = new Artist
            {
                ArtistId = 5,
                StageName = "Funkdoobiest",
                Country = "USA",
                DebutDate = new DateTime(1992, 1, 1),
                Biography = "American hip hop group from Los Angeles.",
                IsActive = true
            };

            var pharcyde = new Artist
            {
                ArtistId = 6,
                StageName = "The Pharcyde",
                Country = "USA",
                DebutDate = new DateTime(1991, 1, 1),
                Biography = "Alternative hip hop group known for playful style.",
                IsActive = true
            };

            var kettama = new Artist
            {
                ArtistId = 7,
                StageName = "Kettama",
                Country = "Ireland",
                DebutDate = new DateTime(2018, 1, 1),
                Biography = "Irish techno and house DJ/producer.",
                IsActive = true
            };

            var charlotte = new Artist
            {
                ArtistId = 8,
                StageName = "Charlotte de Witte",
                Country = "Belgium",
                DebutDate = new DateTime(2010, 1, 1),
                Biography = "Belgian techno DJ and producer.",
                IsActive = true
            };

            var amelie = new Artist
            {
                ArtistId = 9,
                StageName = "Amelie Lens",
                Country = "Belgium",
                DebutDate = new DateTime(2014, 1, 1),
                Biography = "Belgian techno DJ and producer.",
                IsActive = true
            };

            //  ALBUMS 
            var album1 = new Album
            {
                AlbumId = 1,
                Title = "Odd Fellows Rest",
                ReleaseDate = new DateTime(1998, 7, 7),
                Label = "Mayhem Records",
                TotalTracks = 1,
                Rating = 4.5,
                Artist = crowbar
            };

            var album2 = new Album
            {
                AlbumId = 2,
                Title = "Take as Needed for Pain",
                ReleaseDate = new DateTime(1993, 8, 17),
                Label = "Century Media",
                TotalTracks = 1,
                Rating = 4.6,
                Artist = eyehategod
            };

            var album3 = new Album
            {
                AlbumId = 3,
                Title = "When the Kite String Pops",
                ReleaseDate = new DateTime(1994, 8, 1),
                Label = "Rotten Records",
                TotalTracks = 1,
                Rating = 4.9,
                Artist = acidBath
            };

            var album4 = new Album
            {
                AlbumId = 4,
                Title = "Illmatic",
                ReleaseDate = new DateTime(1994, 4, 19),
                Label = "Columbia Records",
                TotalTracks = 1,
                Rating = 5.0,
                Artist = nas
            };

            var album5 = new Album
            {
                AlbumId = 5,
                Title = "Which Doobie U B?",
                ReleaseDate = new DateTime(1992, 7, 28),
                Label = "Epic Records",
                TotalTracks = 1,
                Rating = 4.2,
                Artist = funkdoobiest
            };

            var album6 = new Album
            {
                AlbumId = 6,
                Title = "Bizarre Ride II the Pharcyde",
                ReleaseDate = new DateTime(1992, 11, 24),
                Label = "Delicious Vinyl",
                TotalTracks = 1,
                Rating = 4.7,
                Artist = pharcyde
            };

            var album7 = new Album
            {
                AlbumId = 7,
                Title = "B.O.D.Y EP",
                ReleaseDate = new DateTime(2019, 6, 1),
                Label = "Techno Label",
                TotalTracks = 1,
                Rating = 4.4,
                Artist = kettama
            };

            var album8 = new Album
            {
                AlbumId = 8,
                Title = "Selected Works",
                ReleaseDate = new DateTime(2018, 10, 1),
                Label = "KNTXT",
                TotalTracks = 1,
                Rating = 4.8,
                Artist = charlotte
            };

            var album9 = new Album
            {
                AlbumId = 9,
                Title = "Stay With Me EP",
                ReleaseDate = new DateTime(2019, 5, 1),
                Label = "Lenske",
                TotalTracks = 1,
                Rating = 4.7,
                Artist = amelie
            };

            //  SONGS 
            var song1 = new Song
            {
                SongId = 1,
                Title = "Planets Collide",
                Duration = new TimeSpan(0, 4, 38),
                ReleaseDate = new DateTime(1998, 7, 7),
                PlayCount = 210,
                PopularityScore = 8.7,
                Mood = MoodType.Sad,
                IsExplicit = false,
                Artist = crowbar,
                Album = album1,
                Genre = sludge
            };

            var song2 = new Song
            {
                SongId = 2,
                Title = "Blank",
                Duration = new TimeSpan(0, 5, 10),
                ReleaseDate = new DateTime(1993, 8, 17),
                PlayCount = 170,
                PopularityScore = 8.2,
                Mood = MoodType.Angry,
                IsExplicit = true,
                Artist = eyehategod,
                Album = album2,
                Genre = sludge
            };

            var song3 = new Song
            {
                SongId = 3,
                Title = "Scream of the Butterfly",
                Duration = new TimeSpan(0, 4, 54),
                ReleaseDate = new DateTime(1994, 8, 1),
                PlayCount = 260,
                PopularityScore = 9.4,
                Mood = MoodType.Sad,
                IsExplicit = false,
                Artist = acidBath,
                Album = album3,
                Genre = sludge
            };

            var song4 = new Song
            {
                SongId = 4,
                Title = "N.Y. State of Mind",
                Duration = new TimeSpan(0, 4, 54),
                ReleaseDate = new DateTime(1994, 4, 19),
                PlayCount = 450,
                PopularityScore = 9.9,
                Mood = MoodType.Focus,
                IsExplicit = true,
                Artist = nas,
                Album = album4,
                Genre = hipHop
            };

            var song5 = new Song
            {
                SongId = 5,
                Title = "Rock On",
                Duration = new TimeSpan(0, 3, 42),
                ReleaseDate = new DateTime(1992, 7, 28),
                PlayCount = 140,
                PopularityScore = 7.8,
                Mood = MoodType.Happy,
                IsExplicit = false,
                Artist = funkdoobiest,
                Album = album5,
                Genre = hipHop
            };

            var song6 = new Song
            {
                SongId = 6,
                Title = "Passin' Me By",
                Duration = new TimeSpan(0, 5, 3),
                ReleaseDate = new DateTime(1992, 11, 24),
                PlayCount = 320,
                PopularityScore = 9.1,
                Mood = MoodType.Chill,
                IsExplicit = false,
                Artist = pharcyde,
                Album = album6,
                Genre = hipHop
            };

            var song7 = new Song
            {
                SongId = 7,
                Title = "B.O.D.Y",
                Duration = new TimeSpan(0, 5, 45),
                ReleaseDate = new DateTime(2019, 6, 1),
                PlayCount = 280,
                PopularityScore = 8.8,
                Mood = MoodType.Energetic,
                IsExplicit = false,
                Artist = kettama,
                Album = album7,
                Genre = techno
            };

            var song8 = new Song
            {
                SongId = 8,
                Title = "Selected",
                Duration = new TimeSpan(0, 6, 10),
                ReleaseDate = new DateTime(2018, 10, 1),
                PlayCount = 340,
                PopularityScore = 9.2,
                Mood = MoodType.Energetic,
                IsExplicit = false,
                Artist = charlotte,
                Album = album8,
                Genre = techno
            };

            var song9 = new Song
            {
                SongId = 9,
                Title = "Stay With Me",
                Duration = new TimeSpan(0, 5, 50),
                ReleaseDate = new DateTime(2019, 5, 1),
                PlayCount = 310,
                PopularityScore = 9.0,
                Mood = MoodType.Energetic,
                IsExplicit = false,
                Artist = amelie,
                Album = album9,
                Genre = techno
            };

            crowbar.Albums.Add(album1);
            eyehategod.Albums.Add(album2);
            acidBath.Albums.Add(album3);
            nas.Albums.Add(album4);
            funkdoobiest.Albums.Add(album5);
            pharcyde.Albums.Add(album6);
            kettama.Albums.Add(album7);
            charlotte.Albums.Add(album8);
            amelie.Albums.Add(album9);

            crowbar.Songs.Add(song1);
            eyehategod.Songs.Add(song2);
            acidBath.Songs.Add(song3);
            nas.Songs.Add(song4);
            funkdoobiest.Songs.Add(song5);
            pharcyde.Songs.Add(song6);
            kettama.Songs.Add(song7);
            charlotte.Songs.Add(song8);
            amelie.Songs.Add(song9);

            album1.Songs.Add(song1);
            album2.Songs.Add(song2);
            album3.Songs.Add(song3);
            album4.Songs.Add(song4);
            album5.Songs.Add(song5);
            album6.Songs.Add(song6);
            album7.Songs.Add(song7);
            album8.Songs.Add(song8);
            album9.Songs.Add(song9);

            sludge.Songs.AddRange(new List<Song> { song1, song2, song3 });
            hipHop.Songs.AddRange(new List<Song> { song4, song5, song6 });
            techno.Songs.AddRange(new List<Song> { song7, song8, song9 });

            //  USERS 
            var user1 = new User
            {
                UserId = 1,
                Username = "hrc",
                Email = "hrc@gmail.com",
                RegistrationDate = new DateTime(2024, 10, 10),
                FavoriteGenreName = "Sludge Metal",
                IsPremium = true
            };

            var user2 = new User
            {
                UserId = 2,
                Username = "jurs",
                Email = "jurs@gmail.com",
                RegistrationDate = new DateTime(2025, 1, 15),
                FavoriteGenreName = "Hip Hop",
                IsPremium = false
            };

            //  PLAYLISTS 
            var playlist1 = new Models.Playlist
            {
                PlaylistId = 1,
                Name = "Sludge Doom Vibes",
                Description = "Heavy and slow songs for dark mood.",
                CreatedAt = DateTime.Now.AddDays(-15),
                IsPublic = true,
                Likes = 22,
                Owner = user1
            };

            var playlist2 = new Models.Playlist
            {
                PlaylistId = 2,
                Name = "90s Hip Hop Essentials",
                Description = "Classic rap tracks.",
                CreatedAt = DateTime.Now.AddDays(-10),
                IsPublic = true,
                Likes = 35,
                Owner = user2
            };

            var playlist3 = new Models.Playlist
            {
                PlaylistId = 3,
                Name = "Late Night Techno",
                Description = "Energetic techno tracks for night sessions.",
                CreatedAt = DateTime.Now.AddDays(-5),
                IsPublic = false,
                Likes = 18,
                Owner = user1
            };

            playlist1.Songs.AddRange(new List<Song> { song1, song2, song3 });
            playlist2.Songs.AddRange(new List<Song> { song4, song5, song6 });
            playlist3.Songs.AddRange(new List<Song> { song7, song8, song9 });

            song1.Playlists.Add(playlist1);
            song2.Playlists.Add(playlist1);
            song3.Playlists.Add(playlist1);

            song4.Playlists.Add(playlist2);
            song5.Playlists.Add(playlist2);
            song6.Playlists.Add(playlist2);

            song7.Playlists.Add(playlist3);
            song8.Playlists.Add(playlist3);
            song9.Playlists.Add(playlist3);

            user1.Playlists.AddRange(new List<Models.Playlist> { playlist1, playlist3 });
            user2.Playlists.Add(playlist2);
 
            var history1 = new ListeningHistory
            {
                ListeningHistoryId = 1,
                ListenedAt = DateTime.Now.AddHours(-6),
                Repeats = 2,
                User = user1,
                Song = song1
            };

            var history2 = new ListeningHistory
            {
                ListeningHistoryId = 2,
                ListenedAt = DateTime.Now.AddHours(-4),
                Repeats = 1,
                User = user1,
                Song = song8
            };

            var history3 = new ListeningHistory
            {
                ListeningHistoryId = 3,
                ListenedAt = DateTime.Now.AddHours(-2),
                Repeats = 3,
                User = user2,
                Song = song4
            };

            user1.ListeningHistory.AddRange(new List<ListeningHistory> { history1, history2 });
            user2.ListeningHistory.Add(history3);

            var allSongs = new List<Song>
            {
                song1, song2, song3, song4, song5, song6, song7, song8, song9
            };

            var allArtists = new List<Artist>
            {
                crowbar, eyehategod, acidBath, nas, funkdoobiest, pharcyde, kettama, charlotte, amelie
            };

            var allPlaylists = new List<Models.Playlist>
            {
                playlist1, playlist2, playlist3
            };

            var allUsers = new List<User>
            {
                user1, user2
            };

            var allAlbums = new List<Album>
            {
                album1, album2, album3, album4, album5, album6, album7, album8, album9
            };

            var allGenres = new List<Genre>
            {
                sludge, hipHop, techno
    };

            var allListeningHistories = new List<ListeningHistory>
            {
                history1, history2, history3
            };
            return (allSongs, allArtists, allAlbums, allGenres, allPlaylists, allUsers, allListeningHistories);
        }
    }
}