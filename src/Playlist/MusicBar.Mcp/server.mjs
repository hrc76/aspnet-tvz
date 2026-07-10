import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { z } from 'zod';

const baseUrl = (process.env.MUSICBAR_BASE_URL
  || 'https://musicbar-ht2026.azurewebsites.net').replace(/\/$/, '');

const server = new McpServer({
  name: 'musicbar-catalog',
  version: '1.0.0'
});

async function getJson(path) {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Accept: 'application/json' }
  });

  if (!response.ok) {
    throw new Error(`MusicBar API returned HTTP ${response.status}.`);
  }

  return response.json();
}

function textResult(value) {
  return {
    content: [{ type: 'text', text: JSON.stringify(value, null, 2) }]
  };
}

server.registerTool('search_musicbar', {
  description: 'Search MusicBar pages, songs, artists, albums, genres, public playlists and users.',
  inputSchema: {
    term: z.string().trim().min(2).describe('Search text, for example Nirvana or Rock')
  }
}, async ({ term }) => textResult(
  await getJson(`/global-search?term=${encodeURIComponent(term)}`)
));

server.registerTool('list_albums', {
  description: 'List albums from the live MusicBar catalog.',
  inputSchema: {
    limit: z.number().int().min(1).max(50).default(10)
  }
}, async ({ limit }) => {
  const albums = await getJson('/api/album');
  return textResult(albums.slice(0, limit));
});

server.registerTool('list_songs', {
  description: 'List songs from the live MusicBar catalog.',
  inputSchema: {
    limit: z.number().int().min(1).max(50).default(10)
  }
}, async ({ limit }) => {
  const songs = await getJson('/api/song');
  return textResult(songs.slice(0, limit));
});

const transport = new StdioServerTransport();
await server.connect(transport);
