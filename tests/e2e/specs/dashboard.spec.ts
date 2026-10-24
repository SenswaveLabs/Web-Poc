import { test, expect, waitForApp, TEST_USER, hasTestUser } from './fixtures';

// Uses the real test account. Only reads data: no device state is changed.
test.describe('home dashboard', () => {
  test.skip(!hasTestUser, 'E2E_USER_EMAIL and E2E_USER_PASSWORD are not set');

  test('shows home header actions and device tiles', async ({ page, pageErrors }) => {
    await page.goto('/auth/login');
    await waitForApp(page);

    await page.getByLabel('Email').fill(TEST_USER.email);
    await page.getByLabel('Password').fill(TEST_USER.password);
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();

    const consentGate = page.getByText('Before you continue');
    await expect(page.getByRole('navigation').getByText('Logout', { exact: true }).or(consentGate)).toBeVisible({ timeout: 30_000 });
    if (await consentGate.isVisible()) {
      await page.getByText('I have read and accept').click();
      await page.getByRole('button', { name: 'Accept and continue' }).click();
      await expect(consentGate).toBeHidden();
    }

    // An account without a home is sent from /devices to /homes/list after the dashboard starts loading.
    const addDevice = page.getByRole('button', { name: 'Device', exact: true });
    const joinHome = page.getByText('Join Home', { exact: true });
    await expect(addDevice.or(joinHome)).toBeVisible({ timeout: 30_000 });
    test.skip(await joinHome.isVisible(), 'Test account has no home');

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
});
