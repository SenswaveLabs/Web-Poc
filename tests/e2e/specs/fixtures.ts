import { test as base, expect, Page } from '@playwright/test';

export const API_URL = (process.env.E2E_API_URL ?? 'https://apisenswave.psplace.dev').replace(/\/$/, '');

/** Email on a reserved domain: no real mailbox, so API calls never reach a person. */
export const UNKNOWN_EMAIL = 'e2e-nobody@example.com';

export const VALID_PASSWORD = 'E2e-Password-123!';

/**
 * Points the app at the test API by replacing appsettings*.json in the browser,
 * so the same tests run against `dotnet run` and the Docker image without config changes.
 */
async function useTestApi(page: Page) {
  await page.route(/\/appsettings(\.[A-Za-z]+)?\.json$/, (route) =>
    route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({
        Api: { BaseUrl: API_URL },
        Auth: { Google: { ClientId: 'e2e-client-id.apps.googleusercontent.com' } },
      }),
    }),
  );
}

export const test = base.extend<{ pageErrors: Error[] }>({
  pageErrors: async ({ page }, use) => {
    const errors: Error[] = [];
    page.on('pageerror', (error) => errors.push(error));
    await use(errors);
  },
  page: async ({ page }, use) => {
    await useTestApi(page);
    await use(page);
  },
});

export { expect };

/** Waits until Blazor finished booting and rendered a page heading. */
export async function waitForApp(page: Page) {
  await expect(page.locator('#app h1').first()).toBeVisible({ timeout: 45_000 });
}

export function apiRoute(path: string) {
  return `${API_URL}${path}`;
}

/** Real test account, provided by CI secrets. Tests that need it skip when it is missing. */
export const TEST_USER = {
  email: process.env.E2E_USER_EMAIL ?? '',
  password: process.env.E2E_USER_PASSWORD ?? '',
};

export const hasTestUser = TEST_USER.email !== '' && TEST_USER.password !== '';
