const { test, expect } = require('@playwright/test');

// Demo history hrani analytics dashboard i achievemente na profilu.
test('listener can inspect analytics and achievement progress', async ({ page }) => {
  await page.goto('/Account/Login');
  await page.getByLabel('Email').fill('hrc@gmail.com');
  await page.getByLabel('Password').fill('password');
  await page.getByRole('button', { name: 'Log in' }).click();

  await page.goto('/Analytics');
  await expect(page.getByRole('heading', { name: /your listening/i })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Activity signal' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Top genres' })).toBeVisible();

  await page.goto('/Account/Profile');
  await expect(page.getByRole('heading', { name: 'Achievements' })).toBeVisible();
  await expect(page.locator('.achievement-card')).toHaveCount(9);
  await expect(page.locator('.achievement-card.unlocked').first()).toBeVisible();
  await expect(page.locator('.achievement-card.locked').first()).toBeVisible();
});
