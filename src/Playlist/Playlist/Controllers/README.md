# Controllers

- `Home`, `Discover`, `Library`: ulazne stranice aplikacije.
- `Song`, `Album`, `Artist`, `Genre`, `Playlist`, `User`: MVC CRUD kontroleri.
- `Account`: registracija, lokalna/Google prijava, profil i zabrana pristupa.
- `Favorite`, `SavedAlbum`: male POST akcije za korisničku biblioteku.
- `ListeningHistory`, `Analytics`: bilježenje i analiza slušanja.
- `AiDj`: priprema AI konteksta i spremanje generiranog miksa.
- `GlobalSearch`: jedinstvena pretraga stranica i podataka.
- `Api/`: REST endpointi koje koriste testovi, MCP i vanjski klijenti.

Controller ne bi trebao izravno rješavati spremanje datoteka ili OpenAI protokol; za to koristi Services. Ponavljajući pristup bazi ide kroz Repositories, dok složeniji analitički upiti mogu koristiti DbContext izravno.
