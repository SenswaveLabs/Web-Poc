import { test, expect, waitForApp, TEST_USER, hasTestUser } from './fixtures';

// Uses the real test account from E2E_USER_EMAIL / E2E_USER_PASSWORD. Nothing is mocked.
test.describe('signed in user', () => {
  test.skip(!hasTestUser, 'E2E_USER_EMAIL and E2E_USER_PASSWORD are not set');

  test('signs in, opens the app and signs out', async ({ page, pageErrors }) => {
    await page.goto('/auth/login');
    await waitForApp(page);

    await page.getByLabel('Email').fill(TEST_USER.email);
    await page.getByLabel('Password').fill(TEST_USER.password);
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

    await expect(page).not.toHaveURL(/\/auth\/login/);
    await expect(page.getByRole('alert').filter({ hasText: 'Incorrect email or password.' })).toHaveCount(0);

    await logout.click();

    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
    expect(pageErrors).toEqual([]);
  });
});
