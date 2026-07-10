# MusicBar MCP server

This read-only stdio MCP server exposes the live MusicBar catalog to agentic IDEs.

Tools:

- `search_musicbar` searches pages and catalog data.
- `list_albums` returns albums from the REST API.
- `list_songs` returns songs from the REST API.

VS Code discovers the server through `.vscode/mcp.json`. Run **MCP: List Servers**
and start `musicbar`, then use Agent mode and inspect the available tools.

The server can also be started manually from `src/Playlist` with `npm run mcp`.
