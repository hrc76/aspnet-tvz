const { test, expect } = require('@playwright/test');

// Odabrana slika mora imati lokalni preview, a X mora ponistiti izbor prije uploada.
test('profile image can be previewed and cancelled before upload', async ({ page }) => {
  await page.goto('/Account/Login');
  await page.getByLabel('Email').fill('hrc@gmail.com');
  await page.getByLabel('Password').fill('password');
  await page.getByRole('button', { name: 'Log in' }).click();
  await page.goto('/Account/Profile');

  const input = page.locator('input[name="profileImage"]');
  await input.setInputFiles({
    name: 'preview.png',
    mimeType: 'image/png',
    buffer: Buffer.from('89504e470d0a1a0a0000000d49484452', 'hex')
  });

  const previewShell = page.locator('[data-image-preview-shell]');
  await expect(previewShell).toBeVisible();
  await expect(previewShell.locator('img')).toHaveAttribute('src', /^blob:/);
  await expect(previewShell).toContainText('preview.png');

  await previewShell.getByRole('button', { name: 'Cancel selected image' }).click();
  await expect(previewShell).toBeHidden();
  await expect(input).toHaveValue('');
});
