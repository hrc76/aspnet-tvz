using Playlist.Models;

namespace Playlist
{
    public static class DataSeeder
    {
        private static string SH(int n) =>
            $"https://www.soundhelix.com/examples/mp3/SoundHelix-Song-{n}.mp3";

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
            // ── GENRES ───────────────────────────────────────────────────
            var sludge   = new Genre { GenreId = 1, Name = "Sludge Metal",  Description = "Heavy, slow and aggressive metal genre" };
            var hipHop   = new Genre { GenreId = 2, Name = "Hip Hop",       Description = "Rhythmic music with rap vocals and beats" };
            var techno   = new Genre { GenreId = 3, Name = "Techno",        Description = "Electronic dance music with repetitive beats" };
            var rock     = new Genre { GenreId = 4, Name = "Rock",          Description = "Guitar-driven rock music spanning classic to alternative" };
            var heavyMet = new Genre { GenreId = 5, Name = "Heavy Metal",   Description = "Loud, distorted guitars and powerful rhythms" };
            var electro  = new Genre { GenreId = 6, Name = "Electronic",    Description = "Synthesizer-based music with electronic production" };

            // ── ARTISTS ──────────────────────────────────────────────────
            var crowbar   = new Artist { ArtistId = 1,  StageName = "Crowbar",           Country = "USA",     DebutDate = new DateTime(1990, 1, 1), Biography = "American sludge metal band from New Orleans.",             IsActive = true };
            var ehg       = new Artist { ArtistId = 2,  StageName = "Eyehategod",         Country = "USA",     DebutDate = new DateTime(1988, 1, 1), Biography = "Influential sludge metal band from New Orleans.",          IsActive = true };
            var acidBath  = new Artist { ArtistId = 3,  StageName = "Acid Bath",          Country = "USA",     DebutDate = new DateTime(1991, 1, 1), Biography = "Cult sludge metal band with dark, gothic sound.",          IsActive = false };
            var nas       = new Artist { ArtistId = 4,  StageName = "Nas",                Country = "USA",     DebutDate = new DateTime(1991, 1, 1), Biography = "Legendary hip hop artist from Queensbridge, New York.",     IsActive = true };
            var funkd     = new Artist { ArtistId = 5,  StageName = "Funkdoobiest",        Country = "USA",     DebutDate = new DateTime(1992, 1, 1), Biography = "American hip hop group from Los Angeles.",                 IsActive = true };
            var pharcyde  = new Artist { ArtistId = 6,  StageName = "The Pharcyde",        Country = "USA",     DebutDate = new DateTime(1991, 1, 1), Biography = "Alternative hip hop group known for playful style.",        IsActive = true };
            var kettama   = new Artist { ArtistId = 7,  StageName = "Kettama",             Country = "Ireland", DebutDate = new DateTime(2018, 1, 1), Biography = "Irish techno and house DJ/producer.",                      IsActive = true };
            var charlotte = new Artist { ArtistId = 8,  StageName = "Charlotte de Witte",  Country = "Belgium", DebutDate = new DateTime(2010, 1, 1), Biography = "Belgian techno DJ and producer.",                          IsActive = true };
            var amelie    = new Artist { ArtistId = 9,  StageName = "Amelie Lens",          Country = "Belgium", DebutDate = new DateTime(2014, 1, 1), Biography = "Belgian techno DJ and producer.",                          IsActive = true };
            var jayz      = new Artist { ArtistId = 10, StageName = "Jay-Z",               Country = "USA",     DebutDate = new DateTime(1996, 1, 1), Biography = "Brooklyn rapper and hip hop mogul, one of the best-selling music artists of all time.", IsActive = true };
            var wutang    = new Artist { ArtistId = 11, StageName = "Wu-Tang Clan",         Country = "USA",     DebutDate = new DateTime(1992, 1, 1), Biography = "Legendary hip hop collective from Staten Island, New York.", IsActive = true };
            var metallica = new Artist { ArtistId = 12, StageName = "Metallica",            Country = "USA",     DebutDate = new DateTime(1981, 1, 1), Biography = "Iconic heavy metal band and one of the most successful rock acts of all time.", IsActive = true };
            var sabbath   = new Artist { ArtistId = 13, StageName = "Black Sabbath",        Country = "UK",      DebutDate = new DateTime(1968, 1, 1), Biography = "Pioneers of heavy metal, fronted by Ozzy Osbourne.",         IsActive = false };
            var nirvana   = new Artist { ArtistId = 14, StageName = "Nirvana",              Country = "USA",     DebutDate = new DateTime(1987, 1, 1), Biography = "Grunge rock band that defined a generation, fronted by Kurt Cobain.", IsActive = false };
            var radiohead = new Artist { ArtistId = 15, StageName = "Radiohead",            Country = "UK",      DebutDate = new DateTime(1985, 1, 1), Biography = "Critically acclaimed alternative rock band known for experimental sound.", IsActive = true };
            var daftPunk  = new Artist { ArtistId = 16, StageName = "Daft Punk",            Country = "France",  DebutDate = new DateTime(1993, 1, 1), Biography = "French electronic duo renowned for mixing disco, funk and electronic music.", IsActive = false };

            // ── ALBUMS ───────────────────────────────────────────────────
            var album1  = new Album { AlbumId = 1,  Title = "Odd Fellows Rest",              ReleaseDate = new DateTime(1998, 7,  7),  Label = "Mayhem Records",       TotalTracks = 1, Rating = 4.5, Artist = crowbar,   CoverUrl = null };
            var album2  = new Album { AlbumId = 2,  Title = "Take as Needed for Pain",       ReleaseDate = new DateTime(1993, 8, 17),  Label = "Century Media",        TotalTracks = 1, Rating = 4.6, Artist = ehg,       CoverUrl = null };
            var album3  = new Album { AlbumId = 3,  Title = "When the Kite String Pops",     ReleaseDate = new DateTime(1994, 8,  1),  Label = "Rotten Records",       TotalTracks = 1, Rating = 4.9, Artist = acidBath,  CoverUrl = "https://upload.wikimedia.org/wikipedia/en/2/22/Acidbathkitestringpops.jpg" };
            var album4  = new Album { AlbumId = 4,  Title = "Illmatic",                      ReleaseDate = new DateTime(1994, 4, 19),  Label = "Columbia Records",     TotalTracks = 1, Rating = 5.0, Artist = nas,       CoverUrl = "/images/albums/Illmatic.jpg" };
            var album5  = new Album { AlbumId = 5,  Title = "Which Doobie U B",              ReleaseDate = new DateTime(1992, 7, 28),  Label = "Epic Records",         TotalTracks = 1, Rating = 4.2, Artist = funkd,     CoverUrl = null };
            var album6  = new Album { AlbumId = 6,  Title = "Bizarre Ride II the Pharcyde",  ReleaseDate = new DateTime(1992, 11, 24), Label = "Delicious Vinyl",      TotalTracks = 1, Rating = 4.7, Artist = pharcyde,  CoverUrl = null };
            var album7  = new Album { AlbumId = 7,  Title = "B.O.D.Y EP",                    ReleaseDate = new DateTime(2019, 6,  1),  Label = "Techno Label",         TotalTracks = 1, Rating = 4.4, Artist = kettama,   CoverUrl = null };
            var album8  = new Album { AlbumId = 8,  Title = "Selected Works",                ReleaseDate = new DateTime(2018, 10, 1),  Label = "KNTXT",                TotalTracks = 1, Rating = 4.8, Artist = charlotte, CoverUrl = null };
            var album9  = new Album { AlbumId = 9,  Title = "Stay With Me EP",               ReleaseDate = new DateTime(2019, 5,  1),  Label = "Lenske",               TotalTracks = 1, Rating = 4.7, Artist = amelie,    CoverUrl = null };
            var album10 = new Album { AlbumId = 10, Title = "The Blueprint",                 ReleaseDate = new DateTime(2001, 9, 11),  Label = "Roc-A-Fella Records",  TotalTracks = 3, Rating = 4.9, Artist = jayz,      CoverUrl = "https://upload.wikimedia.org/wikipedia/en/0/0d/Jay-Z_-_The_Blueprint.jpg" };
            var album11 = new Album { AlbumId = 11, Title = "Enter the Wu-Tang: 36 Chambers",ReleaseDate = new DateTime(1993, 11, 9),  Label = "Loud Records",         TotalTracks = 3, Rating = 5.0, Artist = wutang,    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/5/53/WuTangClanEnterthe36Chambers.jpg" };
            var album12 = new Album { AlbumId = 12, Title = "Metallica",                     ReleaseDate = new DateTime(1991, 8, 12),  Label = "Elektra Records",      TotalTracks = 3, Rating = 5.0, Artist = metallica, CoverUrl = "https://upload.wikimedia.org/wikipedia/en/f/f9/Metallica_-_Metallica.jpg" };
            var album13 = new Album { AlbumId = 13, Title = "Paranoid",                      ReleaseDate = new DateTime(1970, 9, 18),  Label = "Vertigo Records",      TotalTracks = 3, Rating = 5.0, Artist = sabbath,   CoverUrl = "https://upload.wikimedia.org/wikipedia/en/0/01/Black_Sabbath_-_Paranoid.jpg" };
            var album14 = new Album { AlbumId = 14, Title = "Nevermind",                     ReleaseDate = new DateTime(1991, 9, 24),  Label = "DGC Records",          TotalTracks = 3, Rating = 5.0, Artist = nirvana,   CoverUrl = "https://upload.wikimedia.org/wikipedia/en/b/b7/NirvanaNevermindalbumcover.jpg" };
            var album15 = new Album { AlbumId = 15, Title = "OK Computer",                   ReleaseDate = new DateTime(1997, 5, 21),  Label = "Parlophone",           TotalTracks = 3, Rating = 4.9, Artist = radiohead, CoverUrl = "https://upload.wikimedia.org/wikipedia/en/b/ba/Radioheadokcomputer.png" };
            var album16 = new Album { AlbumId = 16, Title = "Random Access Memories",        ReleaseDate = new DateTime(2013, 5, 17),  Label = "Columbia Records",     TotalTracks = 3, Rating = 4.8, Artist = daftPunk,  CoverUrl = "https://upload.wikimedia.org/wikipedia/en/a/ae/Random_Access_Memories.jpg" };

            // ── SONGS ────────────────────────────────────────────────────
            // — Sludge Metal (songs 1-3) —
            var song1 = new Song { SongId = 1,  Title = "Planets Collide",            Duration = new TimeSpan(0, 4, 38), ReleaseDate = new DateTime(1998, 7,  7),  PlayCount = 210, PopularityScore = 8.7, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(1),  Artist = crowbar,   Album = album1,  Genre = sludge };
            var song2 = new Song { SongId = 2,  Title = "Blank",                      Duration = new TimeSpan(0, 5, 10), ReleaseDate = new DateTime(1993, 8, 17),  PlayCount = 170, PopularityScore = 8.2, Mood = MoodType.Angry,    IsExplicit = true,  AudioUrl = SH(2),  Artist = ehg,       Album = album2,  Genre = sludge };
            var song3 = new Song { SongId = 3,  Title = "Scream of the Butterfly",    Duration = new TimeSpan(0, 4, 54), ReleaseDate = new DateTime(1994, 8,  1),  PlayCount = 260, PopularityScore = 9.4, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(3),  Artist = acidBath,  Album = album3,  Genre = sludge };
            // — Hip Hop (songs 4-6) —
            var song4 = new Song { SongId = 4,  Title = "N.Y. State of Mind",         Duration = new TimeSpan(0, 4, 54), ReleaseDate = new DateTime(1994, 4, 19),  PlayCount = 450, PopularityScore = 9.9, Mood = MoodType.Focus,    IsExplicit = true,  AudioUrl = SH(4),  Artist = nas,       Album = album4,  Genre = hipHop };
            var song5 = new Song { SongId = 5,  Title = "Rock On",                    Duration = new TimeSpan(0, 3, 42), ReleaseDate = new DateTime(1992, 7, 28),  PlayCount = 140, PopularityScore = 7.8, Mood = MoodType.Happy,    IsExplicit = false, AudioUrl = SH(5),  Artist = funkd,     Album = album5,  Genre = hipHop };
            var song6 = new Song { SongId = 6,  Title = "Passin' Me By",              Duration = new TimeSpan(0, 5,  3), ReleaseDate = new DateTime(1992, 11, 24), PlayCount = 320, PopularityScore = 9.1, Mood = MoodType.Chill,    IsExplicit = false, AudioUrl = SH(6),  Artist = pharcyde,  Album = album6,  Genre = hipHop };
            // — Techno (songs 7-9) —
            var song7 = new Song { SongId = 7,  Title = "B.O.D.Y",                   Duration = new TimeSpan(0, 5, 45), ReleaseDate = new DateTime(2019, 6,  1),  PlayCount = 280, PopularityScore = 8.8, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(7),  Artist = kettama,   Album = album7,  Genre = techno };
            var song8 = new Song { SongId = 8,  Title = "Selected",                   Duration = new TimeSpan(0, 6, 10), ReleaseDate = new DateTime(2018, 10, 1),  PlayCount = 340, PopularityScore = 9.2, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(8),  Artist = charlotte, Album = album8,  Genre = techno };
            var song9 = new Song { SongId = 9,  Title = "Stay With Me",               Duration = new TimeSpan(0, 5, 50), ReleaseDate = new DateTime(2019, 5,  1),  PlayCount = 310, PopularityScore = 9.0, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(9),  Artist = amelie,    Album = album9,  Genre = techno };
            // — Jay-Z: The Blueprint (songs 10-12) —
            var song10 = new Song { SongId = 10, Title = "Izzo (H.O.V.A.)",           Duration = new TimeSpan(0, 3, 47), ReleaseDate = new DateTime(2001, 9, 11),  PlayCount = 560, PopularityScore = 9.6, Mood = MoodType.Happy,    IsExplicit = false, AudioUrl = SH(10), Artist = jayz,      Album = album10, Genre = hipHop };
            var song11 = new Song { SongId = 11, Title = "Girls, Girls, Girls",       Duration = new TimeSpan(0, 4, 35), ReleaseDate = new DateTime(2001, 9, 11),  PlayCount = 430, PopularityScore = 9.0, Mood = MoodType.Happy,    IsExplicit = true,  AudioUrl = SH(11), Artist = jayz,      Album = album10, Genre = hipHop };
            var song12 = new Song { SongId = 12, Title = "Song Cry",                  Duration = new TimeSpan(0, 4, 52), ReleaseDate = new DateTime(2001, 9, 11),  PlayCount = 380, PopularityScore = 9.3, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(12), Artist = jayz,      Album = album10, Genre = hipHop };
            // — Wu-Tang Clan: 36 Chambers (songs 13-15) —
            var song13 = new Song { SongId = 13, Title = "C.R.E.A.M.",                Duration = new TimeSpan(0, 4, 12), ReleaseDate = new DateTime(1993, 11, 9),  PlayCount = 640, PopularityScore = 9.8, Mood = MoodType.Focus,    IsExplicit = false, AudioUrl = SH(13), Artist = wutang,    Album = album11, Genre = hipHop };
            var song14 = new Song { SongId = 14, Title = "Method Man",                Duration = new TimeSpan(0, 5, 50), ReleaseDate = new DateTime(1993, 11, 9),  PlayCount = 510, PopularityScore = 9.5, Mood = MoodType.Energetic, IsExplicit = true,  AudioUrl = SH(14), Artist = wutang,    Album = album11, Genre = hipHop };
            var song15 = new Song { SongId = 15, Title = "Protect Ya Neck",           Duration = new TimeSpan(0, 4, 52), ReleaseDate = new DateTime(1993, 11, 9),  PlayCount = 590, PopularityScore = 9.7, Mood = MoodType.Energetic, IsExplicit = true,  AudioUrl = SH(15), Artist = wutang,    Album = album11, Genre = hipHop };
            // — Metallica: Black Album (songs 16-18) —
            var song16 = new Song { SongId = 16, Title = "Enter Sandman",             Duration = new TimeSpan(0, 5, 31), ReleaseDate = new DateTime(1991, 8, 12),  PlayCount = 870, PopularityScore = 9.9, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(16), Artist = metallica, Album = album12, Genre = heavyMet };
            var song17 = new Song { SongId = 17, Title = "Nothing Else Matters",      Duration = new TimeSpan(0, 6, 28), ReleaseDate = new DateTime(1991, 8, 12),  PlayCount = 750, PopularityScore = 9.7, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(1),  Artist = metallica, Album = album12, Genre = heavyMet };
            var song18 = new Song { SongId = 18, Title = "The Unforgiven",            Duration = new TimeSpan(0, 6, 26), ReleaseDate = new DateTime(1991, 8, 12),  PlayCount = 690, PopularityScore = 9.5, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(2),  Artist = metallica, Album = album12, Genre = heavyMet };
            // — Black Sabbath: Paranoid (songs 19-21) —
            var song19 = new Song { SongId = 19, Title = "War Pigs",                  Duration = new TimeSpan(0, 7, 54), ReleaseDate = new DateTime(1970, 9, 18),  PlayCount = 520, PopularityScore = 9.6, Mood = MoodType.Angry,    IsExplicit = false, AudioUrl = SH(3),  Artist = sabbath,   Album = album13, Genre = heavyMet };
            var song20 = new Song { SongId = 20, Title = "Paranoid",                  Duration = new TimeSpan(0, 2, 48), ReleaseDate = new DateTime(1970, 9, 18),  PlayCount = 610, PopularityScore = 9.7, Mood = MoodType.Angry,    IsExplicit = false, AudioUrl = SH(4),  Artist = sabbath,   Album = album13, Genre = heavyMet };
            var song21 = new Song { SongId = 21, Title = "Iron Man",                  Duration = new TimeSpan(0, 5, 55), ReleaseDate = new DateTime(1970, 9, 18),  PlayCount = 580, PopularityScore = 9.6, Mood = MoodType.Angry,    IsExplicit = false, AudioUrl = SH(5),  Artist = sabbath,   Album = album13, Genre = heavyMet };
            // — Nirvana: Nevermind (songs 22-24) —
            var song22 = new Song { SongId = 22, Title = "Smells Like Teen Spirit",   Duration = new TimeSpan(0, 5,  1), ReleaseDate = new DateTime(1991, 9, 24),  PlayCount = 920, PopularityScore = 9.9, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(6),  Artist = nirvana,   Album = album14, Genre = rock };
            var song23 = new Song { SongId = 23, Title = "Come as You Are",           Duration = new TimeSpan(0, 3, 38), ReleaseDate = new DateTime(1991, 9, 24),  PlayCount = 680, PopularityScore = 9.5, Mood = MoodType.Chill,    IsExplicit = false, AudioUrl = SH(7),  Artist = nirvana,   Album = album14, Genre = rock };
            var song24 = new Song { SongId = 24, Title = "Lithium",                   Duration = new TimeSpan(0, 4, 17), ReleaseDate = new DateTime(1991, 9, 24),  PlayCount = 590, PopularityScore = 9.3, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(8),  Artist = nirvana,   Album = album14, Genre = rock };
            // — Radiohead: OK Computer (songs 25-27) —
            var song25 = new Song { SongId = 25, Title = "Karma Police",              Duration = new TimeSpan(0, 4, 21), ReleaseDate = new DateTime(1997, 5, 21),  PlayCount = 470, PopularityScore = 9.4, Mood = MoodType.Sad,      IsExplicit = false, AudioUrl = SH(9),  Artist = radiohead, Album = album15, Genre = rock };
            var song26 = new Song { SongId = 26, Title = "Paranoid Android",          Duration = new TimeSpan(0, 6, 23), ReleaseDate = new DateTime(1997, 5, 21),  PlayCount = 410, PopularityScore = 9.2, Mood = MoodType.Angry,    IsExplicit = false, AudioUrl = SH(10), Artist = radiohead, Album = album15, Genre = rock };
            var song27 = new Song { SongId = 27, Title = "No Surprises",              Duration = new TimeSpan(0, 3, 48), ReleaseDate = new DateTime(1997, 5, 21),  PlayCount = 390, PopularityScore = 9.1, Mood = MoodType.Chill,    IsExplicit = false, AudioUrl = SH(11), Artist = radiohead, Album = album15, Genre = rock };
            // — Daft Punk: Random Access Memories (songs 28-30) —
            var song28 = new Song { SongId = 28, Title = "Get Lucky",                 Duration = new TimeSpan(0, 6,  9), ReleaseDate = new DateTime(2013, 5, 17),  PlayCount = 840, PopularityScore = 9.8, Mood = MoodType.Happy,    IsExplicit = false, AudioUrl = SH(12), Artist = daftPunk,  Album = album16, Genre = electro };
            var song29 = new Song { SongId = 29, Title = "Instant Crush",             Duration = new TimeSpan(0, 5, 37), ReleaseDate = new DateTime(2013, 5, 17),  PlayCount = 530, PopularityScore = 9.0, Mood = MoodType.Chill,    IsExplicit = false, AudioUrl = SH(13), Artist = daftPunk,  Album = album16, Genre = electro };
            var song30 = new Song { SongId = 30, Title = "Lose Yourself to Dance",    Duration = new TimeSpan(0, 5, 53), ReleaseDate = new DateTime(2013, 5, 17),  PlayCount = 610, PopularityScore = 9.2, Mood = MoodType.Energetic, IsExplicit = false, AudioUrl = SH(14), Artist = daftPunk,  Album = album16, Genre = electro };

            // ── Wire artist ↔ albums ──────────────────────────────────────
            crowbar.Albums.Add(album1);   ehg.Albums.Add(album2);       acidBath.Albums.Add(album3);
            nas.Albums.Add(album4);       funkd.Albums.Add(album5);      pharcyde.Albums.Add(album6);
            kettama.Albums.Add(album7);   charlotte.Albums.Add(album8);  amelie.Albums.Add(album9);
            jayz.Albums.Add(album10);     wutang.Albums.Add(album11);    metallica.Albums.Add(album12);
            sabbath.Albums.Add(album13);  nirvana.Albums.Add(album14);   radiohead.Albums.Add(album15);
            daftPunk.Albums.Add(album16);

            // ── Wire artist ↔ songs ──────────────────────────────────────
            crowbar.Songs.Add(song1);     ehg.Songs.Add(song2);          acidBath.Songs.Add(song3);
            nas.Songs.Add(song4);         funkd.Songs.Add(song5);         pharcyde.Songs.Add(song6);
            kettama.Songs.Add(song7);     charlotte.Songs.Add(song8);     amelie.Songs.Add(song9);
            jayz.Songs.Add(song10);       jayz.Songs.Add(song11);         jayz.Songs.Add(song12);
            wutang.Songs.Add(song13);     wutang.Songs.Add(song14);       wutang.Songs.Add(song15);
            metallica.Songs.Add(song16);  metallica.Songs.Add(song17);    metallica.Songs.Add(song18);
            sabbath.Songs.Add(song19);    sabbath.Songs.Add(song20);      sabbath.Songs.Add(song21);
            nirvana.Songs.Add(song22);    nirvana.Songs.Add(song23);      nirvana.Songs.Add(song24);
            radiohead.Songs.Add(song25);  radiohead.Songs.Add(song26);    radiohead.Songs.Add(song27);
            daftPunk.Songs.Add(song28);   daftPunk.Songs.Add(song29);     daftPunk.Songs.Add(song30);

            // ── Wire album ↔ songs ───────────────────────────────────────
            album1.Songs.Add(song1);    album2.Songs.Add(song2);    album3.Songs.Add(song3);
            album4.Songs.Add(song4);    album5.Songs.Add(song5);    album6.Songs.Add(song6);
            album7.Songs.Add(song7);    album8.Songs.Add(song8);    album9.Songs.Add(song9);
            album10.Songs.Add(song10);  album10.Songs.Add(song11);  album10.Songs.Add(song12);
            album11.Songs.Add(song13);  album11.Songs.Add(song14);  album11.Songs.Add(song15);
            album12.Songs.Add(song16);  album12.Songs.Add(song17);  album12.Songs.Add(song18);
            album13.Songs.Add(song19);  album13.Songs.Add(song20);  album13.Songs.Add(song21);
            album14.Songs.Add(song22);  album14.Songs.Add(song23);  album14.Songs.Add(song24);
            album15.Songs.Add(song25);  album15.Songs.Add(song26);  album15.Songs.Add(song27);
            album16.Songs.Add(song28);  album16.Songs.Add(song29);  album16.Songs.Add(song30);

            // ── Wire genre ↔ songs ───────────────────────────────────────
            sludge.Songs.Add(song1);    sludge.Songs.Add(song2);    sludge.Songs.Add(song3);
            hipHop.Songs.Add(song4);    hipHop.Songs.Add(song5);    hipHop.Songs.Add(song6);
            hipHop.Songs.Add(song10);   hipHop.Songs.Add(song11);   hipHop.Songs.Add(song12);
            hipHop.Songs.Add(song13);   hipHop.Songs.Add(song14);   hipHop.Songs.Add(song15);
            techno.Songs.Add(song7);    techno.Songs.Add(song8);    techno.Songs.Add(song9);
            heavyMet.Songs.Add(song16); heavyMet.Songs.Add(song17); heavyMet.Songs.Add(song18);
            heavyMet.Songs.Add(song19); heavyMet.Songs.Add(song20); heavyMet.Songs.Add(song21);
            rock.Songs.Add(song22);     rock.Songs.Add(song23);     rock.Songs.Add(song24);
            rock.Songs.Add(song25);     rock.Songs.Add(song26);     rock.Songs.Add(song27);
            electro.Songs.Add(song28);  electro.Songs.Add(song29);  electro.Songs.Add(song30);

            // ── USERS ────────────────────────────────────────────────────
            var user1 = new User { UserId = 1, Username = "hrc",  Email = "hrc@gmail.com",   RegistrationDate = new DateTime(2024, 10, 10), FavoriteGenreName = "Sludge Metal", IsPremium = true };
            var user2 = new User { UserId = 2, Username = "jurs", Email = "jurs@gmail.com",  RegistrationDate = new DateTime(2025, 1,  15), FavoriteGenreName = "Hip Hop",       IsPremium = false };

            // ── PLAYLISTS ────────────────────────────────────────────────
            var pl1 = new Models.Playlist { PlaylistId = 1, Name = "Sludge Doom Vibes",       Description = "Heavy and slow songs for dark mood.",              CreatedAt = DateTime.Now.AddDays(-15), IsPublic = true,  Likes = 22, Owner = user1 };
            var pl2 = new Models.Playlist { PlaylistId = 2, Name = "90s Hip Hop Essentials",  Description = "Classic rap tracks from the golden era.",          CreatedAt = DateTime.Now.AddDays(-10), IsPublic = true,  Likes = 35, Owner = user2 };
            var pl3 = new Models.Playlist { PlaylistId = 3, Name = "Late Night Techno",       Description = "Energetic techno tracks for night sessions.",      CreatedAt = DateTime.Now.AddDays(-5),  IsPublic = false, Likes = 18, Owner = user1 };
            var pl4 = new Models.Playlist { PlaylistId = 4, Name = "Heavy Metal Classics",    Description = "Definitive heavy metal tracks for all time.",      CreatedAt = DateTime.Now.AddDays(-3),  IsPublic = true,  Likes = 41, Owner = user1 };
            var pl5 = new Models.Playlist { PlaylistId = 5, Name = "90s Rock Anthems",        Description = "Grunge and alternative rock from the 90s.",        CreatedAt = DateTime.Now.AddDays(-2),  IsPublic = true,  Likes = 29, Owner = user2 };
            var pl6 = new Models.Playlist { PlaylistId = 6, Name = "Electronic Nights",       Description = "Smooth electronic and dance tracks.",              CreatedAt = DateTime.Now.AddDays(-1),  IsPublic = true,  Likes = 15, Owner = user1 };

            // Playlist 1 — sludge
            pl1.Songs.Add(song1);  pl1.Songs.Add(song2);  pl1.Songs.Add(song3);
            song1.Playlists.Add(pl1); song2.Playlists.Add(pl1); song3.Playlists.Add(pl1);

            // Playlist 2 — hip hop (old school)
            pl2.Songs.Add(song4);  pl2.Songs.Add(song5);  pl2.Songs.Add(song6);
            song4.Playlists.Add(pl2); song5.Playlists.Add(pl2); song6.Playlists.Add(pl2);

            // Playlist 3 — techno
            pl3.Songs.Add(song7);  pl3.Songs.Add(song8);  pl3.Songs.Add(song9);
            song7.Playlists.Add(pl3); song8.Playlists.Add(pl3); song9.Playlists.Add(pl3);

            // Playlist 4 — heavy metal
            pl4.Songs.Add(song16); pl4.Songs.Add(song17); pl4.Songs.Add(song18);
            pl4.Songs.Add(song19); pl4.Songs.Add(song20); pl4.Songs.Add(song21);
            song16.Playlists.Add(pl4); song17.Playlists.Add(pl4); song18.Playlists.Add(pl4);
            song19.Playlists.Add(pl4); song20.Playlists.Add(pl4); song21.Playlists.Add(pl4);

            // Playlist 5 — rock
            pl5.Songs.Add(song22); pl5.Songs.Add(song23); pl5.Songs.Add(song24);
            pl5.Songs.Add(song25); pl5.Songs.Add(song26); pl5.Songs.Add(song27);
            song22.Playlists.Add(pl5); song23.Playlists.Add(pl5); song24.Playlists.Add(pl5);
            song25.Playlists.Add(pl5); song26.Playlists.Add(pl5); song27.Playlists.Add(pl5);

            // Playlist 6 — electronic
            pl6.Songs.Add(song28); pl6.Songs.Add(song29); pl6.Songs.Add(song30);
            song28.Playlists.Add(pl6); song29.Playlists.Add(pl6); song30.Playlists.Add(pl6);

            user1.Playlists.Add(pl1); user1.Playlists.Add(pl3); user1.Playlists.Add(pl4); user1.Playlists.Add(pl6);
            user2.Playlists.Add(pl2); user2.Playlists.Add(pl5);

            // ── LISTENING HISTORY ────────────────────────────────────────
            var h1 = new ListeningHistory { ListeningHistoryId = 1, ListenedAt = DateTime.Now.AddHours(-6),  Repeats = 2, User = user1, Song = song1 };
            var h2 = new ListeningHistory { ListeningHistoryId = 2, ListenedAt = DateTime.Now.AddHours(-4),  Repeats = 1, User = user1, Song = song8 };
            var h3 = new ListeningHistory { ListeningHistoryId = 3, ListenedAt = DateTime.Now.AddHours(-2),  Repeats = 3, User = user2, Song = song4 };
            var h4 = new ListeningHistory { ListeningHistoryId = 4, ListenedAt = DateTime.Now.AddHours(-1),  Repeats = 1, User = user1, Song = song16 };
            var h5 = new ListeningHistory { ListeningHistoryId = 5, ListenedAt = DateTime.Now.AddMinutes(-30), Repeats = 2, User = user2, Song = song22 };

            user1.ListeningHistory.Add(h1); user1.ListeningHistory.Add(h2); user1.ListeningHistory.Add(h4);
            user2.ListeningHistory.Add(h3); user2.ListeningHistory.Add(h5);

            return (
                new List<Song>  { song1, song2, song3, song4, song5, song6, song7, song8, song9, song10, song11, song12, song13, song14, song15, song16, song17, song18, song19, song20, song21, song22, song23, song24, song25, song26, song27, song28, song29, song30 },
                new List<Artist>{ crowbar, ehg, acidBath, nas, funkd, pharcyde, kettama, charlotte, amelie, jayz, wutang, metallica, sabbath, nirvana, radiohead, daftPunk },
                new List<Album> { album1, album2, album3, album4, album5, album6, album7, album8, album9, album10, album11, album12, album13, album14, album15, album16 },
                new List<Genre> { sludge, hipHop, techno, rock, heavyMet, electro },
                new List<Models.Playlist> { pl1, pl2, pl3, pl4, pl5, pl6 },
                new List<User>  { user1, user2 },
                new List<ListeningHistory> { h1, h2, h3, h4, h5 }
            );
        }
    }
}
