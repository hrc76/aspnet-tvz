# MusicBar - mapa projekta

Ovaj dokument služi kao brzi vodič kroz rješenje. Projekt zadržava standardni ASP.NET MVC raspored kako bi routing, Razor viewovi i dependency injection ostali predvidljivi.

## Glavne cjeline

```text
src/Playlist/
|-- Playlist/          ASP.NET Core MVC web-aplikacija
|-- Playlist.Tests/    xUnit integracijski i unit testovi
|-- Playlist.E2E/      Playwright testovi stvarnog korisničkog sučelja
|-- MusicBar.Mcp/      MCP server kojim AI agent pristupa MusicBar API-ju
|-- package.json       naredbe za Playwright i MCP test
`-- Playlist.sln       Visual Studio solution
```

## Web-aplikacija `Playlist/`

```text
Controllers/       prima HTTP zahtjev i koordinira ostale slojeve
  Api/             REST API za Song, Album, Artist, Genre, Playlist i User
Data/              EF Core DbContext klase i inicijalizacija baze
Logging/           vlastiti file logging provider
Migrations/        automatski generirane promjene SQL sheme
  Identity/        migracije tablica za prijavu, korisnike i uloge
MockRepositories/  memorijski podaci korišteni u ranijim vježbama
Models/            entiteti koji se spremaju u bazu
Repositories/      sva uobičajena čitanja i promjene podataka
Services/          izdvojena poslovna logika: AI, slike i achievementi
ViewModels/        podaci pripremljeni posebno za view ili API
  Api/             DTO objekti koje REST API prima i vraća
Views/             Razor HTML; mapa odgovara nazivu kontrolera
wwwroot/           CSS, JavaScript, slike i ostale javne datoteke
```

## Gdje je AI?

AI DJ zahtjev prolazi ovim redoslijedom:

1. `Views/AiDj/Index.cshtml` prikazuje formu i rezultat.
2. `Controllers/AiDjController.cs` učitava katalog i profil slušatelja.
3. `Services/IAiDjService.cs` definira ugovor koji kontroler koristi.
4. `Services/OpenAiDjService.cs` šalje strukturirani zahtjev OpenAI Responses API-ju.
5. `ViewModels/AiDjViewModel.cs` sadrži formu, profil, katalog i rezultat.
6. Rezultat sadrži samo ID-eve postojećih MusicBar pjesama; kontroler ih ponovno provjerava prije prikaza i spremanja.

API ključ nije u kodu. Čita se iz `OpenAI:ApiKey` konfiguracije ili `OPENAI_API_KEY` environment varijable.

## Ostale napredne funkcije

- Global search: `GlobalSearchController` + `GlobalSearchResult` + `site.js`.
- Listening history: `player.js` nakon pet sekundi poziva `ListeningHistoryController.RecordPlay`.
- Analytics: `AnalyticsController` iz historyja izrađuje `ListeningAnalyticsViewModel`.
- Achievements: `AchievementService` računa otključane i zaključane značke.
- Queue: stanje i Smart Queue nalaze se u `wwwroot/js/player.js`; Smart Queue koristi `SongController` i `SongRepository`.
- Upload slika: kontroleri pozivaju `ImageStorageService`, koji provjerava veličinu, ekstenziju, MIME tip i stvarni potpis datoteke.
- Autentikacija: `ApplicationDbContext` i ASP.NET Identity; glavni glazbeni podaci koriste `MusicBarDbContext`.
- Logging: `FileLoggerProvider`; lokalno piše u `Logs`, na Azureu u trajni `HOME/data/MusicBar/logs`.
- MCP: `MusicBar.Mcp/server.mjs`; smoke test je `MusicBar.Mcp/smoke-test.mjs`.

## Kako objasniti slojeve

- Controller odlučuje što napraviti za pojedini URL.
- Repository razgovara s bazom preko EF Corea.
- Service obavlja logiku koja nije samo CRUD, primjerice AI ili spremanje slike.
- Model predstavlja podatak u bazi.
- ViewModel predstavlja samo podatke potrebne određenom ekranu.
- View prikazuje HTML i ne bi trebao sadržavati poslovnu logiku.

## Testovi

- `ApiIntegrationTests.cs`: svi API endpointi, CRUD, autentikacija, global search i napredne funkcije.
- `OpenAiDjServiceTests.cs`: validacija strukturiranog AI odgovora.
- `ImageStorageServiceTests.cs`: pravi i lažni upload slike.
- `ListeningHistoryRepositoryTests.cs`: ograničenje historyja na zadnjih 20 stavki.
- `Playlist.E2E/*.spec.js`: stvarni browser scenariji za UI, queue, analytics, AI DJ i brisanje.

Pokretanje:

```powershell
dotnet test .\src\Playlist\Playlist.sln -c Release
npm run test:e2e --prefix .\src\Playlist
npm run mcp:test --prefix .\src\Playlist
```
