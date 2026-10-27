import { test, expect, waitForApp, UNKNOWN_EMAIL, VALID_PASSWORD, apiRoute } from './fixtures';

// Signed-in session is mocked, so the test runs without the test account and changes no data.
test('print view shows page content without navigation and controls', async ({ page, pageErrors }) => {
  await page.route(apiRoute('/api/**'), (route) => route.fulfill({ status: 404 }));
  await page.route(apiRoute('/api/v1/auth/login'), (route) =>
    route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({ tokenType: 'Bearer', accessToken: 'e2e-token', expiresIn: 3600, refreshToken: 'e2e-refresh' }),
    }),
  );
  await page.route(apiRoute('/api/v1/users'), (route) =>
    route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({ id: 'e2e', email: UNKNOWN_EMAIL, theme: 'default', language: 'en', hasActiveConsent: true }),
    }),
  );

  await page.goto('/auth/login');
  await waitForApp(page);
  await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
  await page.getByLabel('Password').fill(VALID_PASSWORD);
  // Remember me keeps the mocked session across the page load below.
  await page.getByText('Remember me').click();
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page).not.toHaveURL(/\/auth\/login/);

  await page.goto('/users/details');
  await waitForApp(page);
  const heading = page.getByRole('heading', { name: 'Settings', level: 1 });
  const logOut = page.getByRole('button', { name: 'Log out' });
  await expect(heading).toBeVisible();
  await expect(logOut).toBeVisible();

  await page.emulateMedia({ media: 'print' });

  await expect(heading).toBeVisible();
  await expect(page.getByText(UNKNOWN_EMAIL)).toBeVisible();
  await expect(page.locator('.print-header')).toBeVisible();
  await expect(page.locator('.mud-drawer')).toBeHidden();
  await expect(logOut).toBeHidden();
  await expect(page.getByText('Appearance', { exact: true })).toBeHidden();
  await expect(page.getByText('Remove account', { exact: true })).toBeHidden();

  // The app layout is a full-height scroll container on screen; on paper it must grow with its content.
  const layout = await page.locator('.main-layout').evaluate((e) => getComputedStyle(e).overflow);
  expect(layout).toBe('visible');

  await page.emulateMedia({ media: 'screen' });
  await expect(page.locator('.print-header')).toBeHidden();
  await expect(logOut).toBeVisible();

  expect(pageErrors).toEqual([]);
});
