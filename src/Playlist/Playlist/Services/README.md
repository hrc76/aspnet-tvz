# Services

- `OpenAiDjService`: OpenAI Responses API, JSON schema odgovor i sigurnosna provjera ID-eva pjesama.
- `IAiDjService`: sučelje koje odvaja kontroler od konkretne AI implementacije i olakšava testiranje.
- `ImageStorageService`: siguran upload profilnih, albumskih i playlist slika.
- `FileStorageOptions`: lokacija trajnog upload direktorija lokalno i na Azureu.
- `AchievementService`: računa napredak znački iz historyja, favorita i playlista.

Service je mjesto za poslovnu logiku koja ne pripada HTML-u, kontroleru ili običnom CRUD repositoryju.
