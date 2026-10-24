import { test, expect, waitForApp, UNKNOWN_EMAIL, VALID_PASSWORD, apiRoute } from './fixtures';

test.describe('forgot password', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/auth/forgot-password');
    await waitForApp(page);
  });

  // Real API: unknown email returns 200 and sends nothing, invalid code returns InvalidToken.
  test('sends code and rejects invalid code (real API)', async ({ page }) => {
    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByRole('button', { name: 'Send code' }).click();

    await expect(page.getByRole('heading', { name: 'Enter reset code' })).toBeVisible();
    await expect(page.getByText(`If an account exists for ${UNKNOWN_EMAIL}`)).toBeVisible();

    await page.getByLabel('Reset code').fill('000000');
    await page.getByLabel('New password', { exact: true }).fill(VALID_PASSWORD);
    await page.getByLabel('Confirm new password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Set new password' }).click();

    await expect(page.getByRole('alert').filter({ hasText: 'The code is invalid or has expired.' })).toBeVisible();
  });

  test('redirects to sign in after successful reset', async ({ page }) => {
    await page.route(apiRoute('/api/v1/auth/forgotPassword'), (route) => route.fulfill({ status: 200 }));
    await page.route(apiRoute('/api/v1/auth/resetPassword'), (route) => route.fulfill({ status: 200 }));

    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByRole('button', { name: 'Send code' }).click();
    await page.getByLabel('Reset code').fill('123456');
    await page.getByLabel('New password', { exact: true }).fill(VALID_PASSWORD);
    await page.getByLabel('Confirm new password').fill(VALID_PASSWORD);
    await page.getByRole('button', { name: 'Set new password' }).click();

    await expect(page).toHaveURL(/\/auth\/login\?passwordReset=true/);
    await expect(page.getByText('Password changed.')).toBeVisible();
  });

  test('can go back and use a different email', async ({ page }) => {
    await page.route(apiRoute('/api/v1/auth/forgotPassword'), (route) => route.fulfill({ status: 200 }));

    await page.getByLabel('Email').fill(UNKNOWN_EMAIL);
    await page.getByRole('button', { name: 'Send code' }).click();
    await page.getByRole('button', { name: 'Use a different email' }).click();

    await expect(page.getByRole('heading', { name: 'Reset password' })).toBeVisible();
    await expect(page.getByLabel('Email')).toHaveValue(UNKNOWN_EMAIL);
  });
});
