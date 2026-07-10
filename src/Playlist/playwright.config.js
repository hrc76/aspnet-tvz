const { defineConfig, devices } = require('@playwright/test');

const remoteBaseUrl = process.env.PLAYWRIGHT_BASE_URL;

module.exports = defineConfig({
  testDir: './Playlist.E2E',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: remoteBaseUrl || 'https://localhost:7086',
    ignoreHTTPSErrors: true,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure'
  },
  projects: [
    {
      name: 'Microsoft Edge',
      use: { ...devices['Desktop Edge'], channel: 'msedge' }
    }
  ],
  webServer: remoteBaseUrl ? undefined : {
    command: 'dotnet run --project Playlist/Playlist.csproj --no-build --launch-profile Playlist',
    url: 'https://localhost:7086',
    ignoreHTTPSErrors: true,
    reuseExistingServer: true,
    timeout: 120_000
  }
});
