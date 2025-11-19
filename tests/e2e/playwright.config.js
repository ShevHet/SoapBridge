// @ts-check
const { defineConfig, devices } = require('@playwright/test');
const path = require('path');

// Determine project directory from environment or use default
const projectDir = process.env.PROJECT_DIR || 'IcutechTestApi';
// Calculate absolute path from tests/e2e to project directory
const projectPath = path.resolve(__dirname, '..', '..', projectDir);

/**
 * @see https://playwright.dev/docs/test-configuration
 */
module.exports = defineConfig({
  testDir: './specs',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: process.env.FRONTEND_URL || 'http://localhost:5030',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],

  webServer: {
    command: `cd ${JSON.stringify(projectPath)} && dotnet run`,
    url: 'http://localhost:5030',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});

