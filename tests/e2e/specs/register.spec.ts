import { test, expect, waitForApp, UNKNOWN_EMAIL, VALID_PASSWORD, apiRoute } from './fixtures';

test.describe('register', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/auth/register');
    await waitForApp(page);
  });

  test('validates password rules and confirmation', async ({ page }) => {
    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByLabel('Password', { exact: true }).fill('short');
    await page.getByLabel('Confirm password').fill('different');
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByText('Password must be at least 10 characters long.')).toBeVisible();
    await expect(page.getByText("Passwords don't match.")).toBeVisible();
  });

  // Success and duplicate responses are mocked so the test never creates accounts on the shared API.
  test('shows check inbox state after successful registration', async ({ page }) => {
    let body: unknown;
    await page.route(apiRoute('/api/v1/auth/register'), (route) => {
      body = route.request().postDataJSON();
      return route.fulfill({ status: 200 });
    });

    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByLabel('Password', { exact: true }).fill(VALID_PASSWORD);
    await page.getByLabel('Confirm password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Create account' }).click();

    await expect(page.getByRole('heading', { name: 'Check your inbox' })).toBeVisible();
    await expect(page.getByText(UNKNOWN_EMAIL)).toBeVisible();
    await expect(page.getByRole('button', { name: /Resend email \(\d+s\)/ })).toBeDisabled();
    expect(body).toEqual({ email: UNKNOWN_EMAIL, password: VALID_PASSWORD });
  });

  test('explains when email is already taken', async ({ page }) => {
    await page.route(apiRoute('/api/v1/auth/register'), (route) =>
      route.fulfill({
        status: 400,
        contentType: 'application/problem+json',
        body: JSON.stringify({ status: 400, errors: { DuplicateUserName: [`Username '${UNKNOWN_EMAIL}' is already taken.`] } }),
      }),
    );

    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByLabel('Password', { exact: true }).fill(VALID_PASSWORD);
    await page.getByLabel('Confirm password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Create account' }).click();

    const alert = page.getByRole('alert').filter({ hasText: 'An account with this email already exists.' });
    await expect(alert).toBeVisible();
    await expect(alert.getByRole('link', { name: 'reset your password' })).toBeVisible();
  });
});
