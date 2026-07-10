const { test, expect } = require('@playwright/test');

// Ovaj scenarij provjerava prijavu, Library funkcije i sigurno kaskadno brisanje.
test('admin can save albums and safely delete catalog data', async ({ page }) => {
  // Prijava s Admin racunom.
  await page.goto('/Account/Login');
  await page.getByLabel('Email').fill('admin@musicbar.local');
  await page.getByLabel('Password').fill('Admin!12345');
  await page.getByRole('button', { name: 'Log in' }).click();
  await expect(page).toHaveURL(/\/$/);

  // Album najprije dovodimo u poznato stanje, zatim testiramo Save to Library.
  await page.goto('/Album/Details/14');
  const removeButton = page.getByRole('button', { name: 'Remove from Library' });
  if (await removeButton.isVisible()) {
    await removeButton.click();
    await page.goto('/Album/Details/14');
  }

  await page.getByRole('button', { name: 'Save to Library' }).click();
  await expect(page.getByRole('button', { name: 'Remove from Library' })).toBeVisible();

  // Provjeravamo da je album stvarno prikazan u Libraryju.
  await page.goto('/Library');
  await expect(page.getByText('Nevermind', { exact: true }).first()).toBeVisible();

  // Provjeravamo i suprotnu operaciju: Remove from Library.
  await page.goto('/Album/Details/14');
  await page.getByRole('button', { name: 'Remove from Library' }).click();
  await page.goto('/Library');
  await expect(page.getByText('Nevermind', { exact: true })).toHaveCount(0);

  // Stvaramo privremeni album i pjesmu iskljucivo za test brisanja.
  const unique = Date.now().toString();
  const albumResponse = await page.request.post('/api/album', {
    data: {
      title: `Deletion Test Album ${unique}`,
      releaseDate: '2026-01-01T00:00:00',
      label: 'E2E Test',
      totalTracks: 1,
      rating: 4.0,
      artistId: 14
    }
  });
  expect(albumResponse.status()).toBe(201);
  const album = await albumResponse.json();

  const songResponse = await page.request.post('/api/song', {
    data: {
      title: `Deletion Test Song ${unique}`,
      duration: '00:03:00',
      releaseDate: '2026-01-01T00:00:00',
      playCount: 0,
      popularityScore: 50,
      mood: 2,
      isExplicit: false,
      artistId: 14,
      albumId: album.albumId,
      genreId: 5
    }
  });
  expect(songResponse.status()).toBe(201);
  const song = await songResponse.json();

  // Brisanje albuma mora obrisati i njegovu povezanu testnu pjesmu.
  const deleteResponse = await page.request.delete(`/api/album/${album.albumId}`);
  expect(deleteResponse.status()).toBe(204);
  expect((await page.request.get(`/api/album/${album.albumId}`)).status()).toBe(404);
  expect((await page.request.get(`/api/song/${song.songId}`)).status()).toBe(404);
});
