const { test, expect } = require('@playwright/test');

// Korisnik moze dodati pjesmu u trajni player queue i njime upravljati.
test('user can add and remove a song from the playback queue', async ({ page }) => {
  await page.goto('/Song/Details/22');
  await page.getByRole('button', { name: /add to queue/i }).click();
  await expect(page.locator('#playerQueueCount')).toHaveText('1');

  await page.locator('#playerQueueToggle').click();
  await expect(page.locator('#playerQueueDrawer')).toBeVisible();
  await expect(page.locator('.player-queue-item')).toContainText('Smells Like Teen Spirit');
  await expect(page.locator('#playerQueueDrawer')).toHaveClass(/is-open/);
  await expect(page.locator('.queue-pixel-eq i')).toHaveCount(8);

  await page.locator('.player-queue-item-remove').click();
  await expect(page.locator('#playerQueueCount')).toHaveText('0');
  await expect(page.locator('#playerQueueEmpty')).toBeVisible();

  await page.locator('#playerQueueClose').click();
  await expect(page.locator('#playerQueueDrawer')).toBeHidden();
});

test('queue control stays visible after selecting and playing a song', async ({ page }) => {
  await page.goto('/Song/Details/22');
  await page.locator('.js-play-btn').click();

  await expect(page.locator('#playerQueueToggle')).toBeVisible();
  await expect(page.locator('#playerQueueToggle')).toContainText('Q:');
  await page.locator('#playerQueueToggle').click();
  await expect(page.locator('#playerQueueDrawer')).toBeVisible();
});

// Glasnoca i mute ostaju isti nakon prelaska na drugu stranicu.
test('player remembers volume between pages', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('#playerVolume')).toHaveValue('50');
  await page.locator('#playerVolume').evaluate((slider) => {
    slider.value = '32';
    slider.dispatchEvent(new Event('input', { bubbles: true }));
  });

  await page.goto('/Discover');
  await expect(page.locator('#playerVolume')).toHaveValue('32');
});
