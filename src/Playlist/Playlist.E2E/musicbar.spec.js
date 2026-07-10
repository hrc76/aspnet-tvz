const { test, expect } = require('@playwright/test');

// Playwright test glumi stvarnog korisnika u browseru kroz 12 jasnih koraka.
test('global search and responsive music discovery journey', async ({ page }) => {
  // 1-2: Otvaranje aplikacije i provjera glavnog sadrzaja.
  await test.step('1. Open the MusicBar home page', async () => {
    await page.goto('/');
    await expect(page).toHaveTitle(/MusicBar/);
  });

  await test.step('2. Verify the main music dashboard loaded', async () => {
    await expect(page.getByRole('heading', { name: 'Listen. Collect. Discover.' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Top Songs' })).toBeVisible();
  });

  // 3-4: Global search mora raditi preko tipkovnice i pronaci Nirvanu.
  await test.step('3. Focus global search with the keyboard shortcut', async () => {
    await page.keyboard.press('Control+K');
    await expect(page.locator('#globalSearchInput')).toBeFocused();
  });

  await test.step('4. Search across the complete catalog', async () => {
    await page.locator('#globalSearchInput').fill('Nirvana');
    await expect(page.locator('.global-search-results')).toHaveClass(/visible/);
    await expect(page.locator('.global-search-result')).toHaveCount(5);
  });

  // 5-6: Otvaranje i provjera detalja izvodaca.
  await test.step('5. Open an artist from global results', async () => {
    const artistResult = page.locator('.global-search-result')
      .filter({ has: page.locator('.global-search-type', { hasText: /^Artist$/ }) })
      .filter({ has: page.locator('strong', { hasText: /^Nirvana$/ }) });
    await artistResult.click();
    await expect(page).toHaveURL(/\/Artist\/Details\/14$/);
  });

  await test.step('6. Verify the artist detail page', async () => {
    await expect(page.getByRole('heading', { name: 'Nirvana', exact: true })).toBeVisible();
    await expect(page.getByText(/USA/).first()).toBeVisible();
  });

  // 7-8: Pretraga i provjera detalja albuma Nevermind.
  await test.step('7. Search for an album from another page', async () => {
    await page.locator('#globalSearchInput').fill('Nevermind');
    const albumResult = page.locator('.global-search-result')
      .filter({ has: page.locator('.global-search-type', { hasText: /^Album$/ }) })
      .filter({ has: page.locator('strong', { hasText: /^Nevermind$/ }) });
    await expect(albumResult).toBeVisible();
    await albumResult.click();
  });

  await test.step('8. Verify the album detail page', async () => {
    await expect(page).toHaveURL(/\/Album\/Details\/14$/);
    await expect(page.getByRole('heading', { name: 'Nevermind', exact: true })).toBeVisible();
  });

  // 9-11: Pretraga pjesme i otvaranje njezinih detalja.
  await test.step('9. Navigate to the song catalog', async () => {
    await page.locator('nav a[href="/Song"]').click();
    await expect(page).toHaveURL(/\/Song$/);
    await expect(page.getByRole('heading', { name: 'Songs', exact: true })).toBeVisible();
  });

  await test.step('10. Use catalog search to find a song', async () => {
    await page.locator('#searchInput').fill('Smells Like Teen Spirit');
    const result = page.locator('#searchResults .search-result-item').first();
    await expect(result).toContainText('Smells Like Teen Spirit');
    await result.click();
  });

  await test.step('11. Verify the selected song details', async () => {
    await expect(page).toHaveURL(/\/Song\/Details\/22$/);
    await expect(page.getByRole('heading', { name: /Smells Like Teen Spirit/ })).toBeVisible();
  });

  // 12: Na mobilnoj sirini sidebar i globalna pretraga moraju ostati dostupni.
  await test.step('12. Verify the responsive mobile navigation', async () => {
    await page.setViewportSize({ width: 390, height: 844 });
    const menuButton = page.locator('#sidebarToggle');
    await expect(menuButton).toBeVisible();
    await menuButton.click();
    await expect(page.locator('#appSidebar')).toHaveClass(/open/);
    await expect(page.locator('#globalSearchInput')).toBeVisible();
  });
});
