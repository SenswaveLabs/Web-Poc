import { test, expect, waitForApp, UNKNOWN_EMAIL, VALID_PASSWORD, apiRoute } from './fixtures';

test.describe('sign in', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/auth/login');
    await waitForApp(page);
  });

  test('shows required field errors on empty submit', async ({ page }) => {
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();

    await expect(page.getByText('Email is required.')).toBeVisible();
    await expect(page.getByText('Password is required.')).toBeVisible();
  });

  test('rejects wrong credentials (real API)', async ({ page }) => {
    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByLabel('Password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();

    await expect(page.getByRole('alert').filter({ hasText: 'Incorrect email or password.' })).toBeVisible();
  });

  test('offers resend when email is not confirmed', async ({ page }) => {
    await page.route(apiRoute('/api/v1/auth/login'), (route) =>
      route.fulfill({ status: 401, contentType: 'application/problem+json', body: JSON.stringify({ status: 401, detail: 'NotAllowed' }) }),
    );

    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByLabel('Password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Sign in', exact: true }).click();

    await expect(page.getByText('Your email is not confirmed yet.')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Resend confirmation email' })).toBeVisible();
  });

  test('links to register and forgot password', async ({ page }) => {
    await page.getByRole('link', { name: 'Create one' }).click();
    await expect(page.getByRole('heading', { name: 'Create account' })).toBeVisible();

    await page.getByRole('link', { name: 'Sign in' }).click();
    await page.getByRole('link', { name: 'Forgot password?' }).click();
    await expect(page.getByRole('heading', { name: 'Reset password' })).toBeVisible();
  });
});

test.describe('sign in banners', () => {
  const cases = [
    { query: 'emailConfirmed=true', text: 'Email confirmed. You can sign in now.' },
    { query: 'emailConfirmed=false', text: 'The confirmation link is invalid or has expired.' },
    { query: 'passwordReset=true', text: 'Password changed. Sign in with your new password.' },
  ];

  for (const { query, text } of cases) {
    test(`shows banner for ?${query}`, async ({ page }) => {
      await page.goto(`/auth/login?${query}`);
      await waitForApp(page);

      await expect(page.getByRole('alert').filter({ hasText: text })).toBeVisible();
    });
  }
});
