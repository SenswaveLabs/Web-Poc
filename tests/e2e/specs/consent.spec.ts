import { test, expect, waitForApp, UNKNOWN_EMAIL, VALID_PASSWORD, apiRoute } from './fixtures';

// Login and user endpoints are mocked to simulate a fresh account. Legal documents come from the real API.
test('new user must accept terms before using the app', async ({ page }) => {
  let consentsPosted = false;

  await page.route(apiRoute('/api/v1/auth/login'), (route) =>
    route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({ tokenType: 'Bearer', accessToken: 'e2e-token', expiresIn: 3600, refreshToken: 'e2e-refresh' }),
    }),
  );
  await page.route(apiRoute('/api/v1/users'), (route) =>
    route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({ id: 'e2e', email: UNKNOWN_EMAIL, theme: 'default', language: 'en', hasActiveConsent: consentsPosted }),
    }),
  );
  await page.route(apiRoute('/api/v1/users/consents'), (route) => {
    consentsPosted = true;
    return route.fulfill({ status: 204 });
  });

  await page.goto('/auth/login');
  await waitForApp(page);
  await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
  await page.getByLabel('Password').fill(VALID_PASSWORD);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();

  await expect(page.getByText('Before you continue')).toBeVisible();

  const accept = page.getByRole('button', { name: 'Accept and continue' });
  await expect(accept).toBeDisabled();

  // Terms are markdown: expect rendered heading, not raw "#" text.
  await page.getByText(/^Terms and Conditions \(v/).click();
  await expect(page.locator('.sens-markdown h1, .sens-markdown h2').first()).toBeVisible();
  await expect(page.locator('.sens-markdown').first()).not.toContainText('# Terms');

  await page.getByText('I have read and accept').click();
  await accept.click();

  await expect(page.getByText('Before you continue')).toBeHidden();
  expect(consentsPosted).toBe(true);
});
