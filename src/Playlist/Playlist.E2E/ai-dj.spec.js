const { test, expect } = require('@playwright/test');

// Provjerava da prijavljeni korisnik vidi novu personaliziranu AI DJ stranicu.
test('authenticated user can open the AI DJ experience', async ({ page }) => {
  await page.goto('/Account/Login');
  await page.getByLabel('Email').fill('hrc@gmail.com');
  await page.getByLabel('Password').fill('password');
  await page.getByRole('button', { name: 'Log in' }).click();
  await expect(page).toHaveURL(/\/$/);

  await page.getByRole('link', { name: /AI DJ/ }).click();
  await expect(page).toHaveURL(/\/AiDj$/);
  await expect(page.getByRole('heading', { name: 'AI DJ', exact: true })).toBeVisible();
  await expect(page.getByLabel('What should we play?')).toBeVisible();
  await expect(page.getByText(/selects only songs that really exist in MusicBar/i)).toBeVisible();
  await expect(page.getByText('AI Music Import')).toHaveCount(0);
});
