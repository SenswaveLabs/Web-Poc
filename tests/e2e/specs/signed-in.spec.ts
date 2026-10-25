import { test, expect, waitForApp, TEST_USER, hasTestUser } from './fixtures';
import type { Page } from '@playwright/test';

// Uses the real test account from E2E_USER_EMAIL / E2E_USER_PASSWORD. Nothing is mocked and no data is changed.
// Serial: the tests share one account and one of them signs out, which would end sessions of parallel tests.
test.describe.configure({ mode: 'serial' });

async function signIn(page: Page, path: string, rememberMe = false) {
  await page.goto(path);
  await waitForApp(page);

  await page.getByLabel('Email').fill(TEST_USER.email);
  await page.getByLabel('Password').fill(TEST_USER.password);
  if (rememberMe) {
    await page.getByText('Remember me').click();
  }
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();

  // Logout is a nav item without href, so it has no link role.
  const logout = page.getByRole('navigation').getByText('Logout', { exact: true });
  const consentGate = page.getByText('Before you continue');

  // A new terms version can require the test account to accept consents again.
  await expect(logout.or(consentGate)).toBeVisible({ timeout: 30_000 });
  if (await consentGate.isVisible()) {
    await page.getByText('I have read and accept').click();
    await page.getByRole('button', { name: 'Accept and continue' }).click();
    await expect(consentGate).toBeHidden();
  }

  return logout;
}

test.describe('signed in user', () => {
  test.skip(!hasTestUser, 'E2E_USER_EMAIL and E2E_USER_PASSWORD are not set');

  test('signs in, opens the app and signs out', async ({ page, pageErrors }) => {
    const logout = await signIn(page, '/auth/login');

    await expect(page).not.toHaveURL(/\/auth\/login/);
    await expect(page.getByRole('alert').filter({ hasText: 'Incorrect email or password.' })).toHaveCount(0);

    await logout.click();

    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
    expect(pageErrors).toEqual([]);
  });

  test('shows home header actions and device tiles', async ({ page, pageErrors }) => {
    await signIn(page, '/auth/login');

    // An account without a home is sent from /devices to /homes/list after the dashboard starts loading.
    const addDevice = page.getByRole('button', { name: 'Device', exact: true });
    const homesPage = page.getByRole('heading', { name: /^(Welcome to Senswave|My homes)$/ });
    await expect(addDevice.or(homesPage)).toBeVisible({ timeout: 30_000 });
    test.skip(await homesPage.isVisible(), 'Test account has no home');

    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Refresh devices' })).toBeVisible();
    await expect(page.getByText('Add device', { exact: true })).toBeVisible();

    const tiles = page.locator('.sens-device');
    if (await tiles.count() > 0) {
      await tiles.first().getByRole('button', { name: /options$/ }).click();
      await expect(page.getByText('Configure', { exact: true })).toBeVisible();
    }

    expect(pageErrors).toEqual([]);
  });

  test('returns to the requested page after sign in and after reload', async ({ page }) => {
    await signIn(page, '/users/details', true);

    const settings = page.getByRole('heading', { name: 'Settings', level: 1 });

    await expect(page).toHaveURL(/\/users\/details$/);
    await expect(settings).toBeVisible();

    await page.reload();
    await waitForApp(page);

    await expect(page).toHaveURL(/\/users\/details$/);
    await expect(settings).toBeVisible();
  });
});
