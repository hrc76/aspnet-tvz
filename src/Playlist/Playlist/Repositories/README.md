# Repositories

Repository skriva ponavljajuće EF Core upite i CRUD operacije od kontrolera. Svaki glavni entitet ima svoj repository. `ListeningHistoryRepository` dodatno provodi pravilo da svaki korisnik zadržava samo posljednjih 20 slušanja.

`Include` učitava povezane podatke koji su potrebni viewu, primjerice Song -> Artist. `AsNoTracking` se koristi za read-only upite kada promjene entiteta neće biti spremljene.
