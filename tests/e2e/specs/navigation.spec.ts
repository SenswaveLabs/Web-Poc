import { test, expect, waitForApp } from './fixtures';

test('protected route keeps the requested page as returnUrl', async ({ page }) => {
  await page.goto('/users/details');
  await waitForApp(page);

  await expect(page).toHaveURL(/\/auth\/login\?returnUrl=%2Fusers%2Fdetails$/);
});
