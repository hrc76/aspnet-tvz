import { fileURLToPath } from 'node:url';
import { Client } from '@modelcontextprotocol/sdk/client/index.js';
import { StdioClientTransport } from '@modelcontextprotocol/sdk/client/stdio.js';

const serverPath = fileURLToPath(new URL('./server.mjs', import.meta.url));

// Testni MCP klijent pokrece nas server preko standardnog stdio transporta.
const transport = new StdioClientTransport({
  command: process.execPath,
  args: [serverPath]
});
const client = new Client({ name: 'musicbar-smoke-test', version: '1.0.0' });

try {
  // Provjeravamo MCP povezivanje, popis alata i stvarnu pretragu Azure kataloga.
  await client.connect(transport);
  const { tools } = await client.listTools();
  const result = await client.callTool({
    name: 'search_musicbar',
    arguments: { term: 'Nirvana' }
  });

  if (tools.length !== 3 || result.isError) {
    throw new Error('Unexpected MCP response.');
  }

  console.log(`MCP OK: ${tools.map(tool => tool.name).join(', ')}`);
  console.log(`Search result contains Nirvana: ${result.content[0].text.includes('Nirvana')}`);
} finally {
  // Vezu uvijek zatvaramo, cak i ako test ne uspije.
  await client.close();
}
