# Data

- `MusicBarDbContext`: glazbeni katalog i domenski korisnički podaci.
- `ApplicationDbContext`: ASP.NET Identity tablice za login, role i Google račun.
- `DbInitializer`: pokreće migracije, početni katalog i demo listening history.

Postoje dva DbContexta jer Identity ima vlastiti standardni model, dok MusicBar ima zaseban domenski model. Oba na Azureu koriste isti connection string, ali različite skupove tablica.
