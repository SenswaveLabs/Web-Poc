import { defineConfig, devices } from '@playwright/test';

// Web app under test. Default starts the app with `dotnet run`; set E2E_BASE_URL to test a running instance (e.g. Docker on :7162).
const baseURL = process.env.E2E_BASE_URL ?? 'http://localhost:5246';

export default defineConfig({
  testDir: './specs',
  timeout: 60_000,
  expect: { timeout: 15_000 },
  fullyParallel: true,
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL,
    // Service worker would serve cached files and hide requests from page.route.
    serviceWorkers: 'block',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        // Test API does not allow localhost origins in CORS. Test-only workaround.
        launchOptions: { args: ['--disable-web-security'] },
      },
    },
    {
      name: 'mobile',
      use: {
        ...devices['Pixel 7'],
        launchOptions: { args: ['--disable-web-security'] },
      },
      testMatch: /startup\.spec\.ts/,
    },
  ],
  webServer: process.env.E2E_BASE_URL
    ? undefined
    : {
        command: 'dotnet run --project ../../src/Senswave.Web/Senswave.Web.csproj --launch-profile http',
        url: baseURL,
        reuseExistingServer: true,
        timeout: 180_000,
      },
});
