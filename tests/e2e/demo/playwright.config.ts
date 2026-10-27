import { defineConfig } from '@playwright/test';

// Demo recording only. The browser is launched by the test itself (persistent profile, headed, screen recording).
export default defineConfig({
  testDir: '.',
  testMatch: /demo\.spec\.ts$/,
  timeout: 30 * 60_000,
  expect: { timeout: 20_000 },
  workers: 1,
  retries: 0,
  reporter: [['list']],
  outputDir: './out/test-results',
});
