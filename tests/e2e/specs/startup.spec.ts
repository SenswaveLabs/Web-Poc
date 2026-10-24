import { test, expect, waitForApp } from './fixtures';

test.describe('startup', () => {
  test('anonymous user lands on sign in page', async ({ page, pageErrors }) => {
    await page.goto('/');
    await waitForApp(page);

    await expect(page).toHaveURL(/\/auth\/login/);
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
    await expect(page).toHaveTitle(/Sign in/);
    expect(pageErrors).toEqual([]);
  });

  test('protected route redirects to sign in', async ({ page }) => {
    await page.goto('/devices');
    await waitForApp(page);

    await expect(page).toHaveURL(/\/auth\/login/);
  });

  test('sign in page has no horizontal scroll', async ({ page }) => {
    await page.goto('/auth/login');
    await waitForApp(page);

    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth);
    expect(overflow).toBeLessThanOrEqual(0);
  });
});
