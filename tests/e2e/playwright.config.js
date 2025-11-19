// @ts-check
const { defineConfig, devices } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

// Determine project directory from environment or use default
let projectDir = process.env.PROJECT_DIR || 'IcutechTestApi';

// If PROJECT_DIR is '.' (root), try to find the actual project directory
if (projectDir === '.') {
  const rootPath = path.resolve(__dirname, '..', '..');
  // Try IcutechTestApi first
  const icutechPath = path.join(rootPath, 'IcutechTestApi');
  if (fs.existsSync(icutechPath)) {
    const csprojFiles = fs.readdirSync(icutechPath).filter(f => f.endsWith('.csproj') && !f.includes('Tests'));
    if (csprojFiles.length > 0) {
      projectDir = 'IcutechTestApi';
    }
  }
  
  // If still '.', try to find any .csproj file (excluding Tests)
  if (projectDir === '.') {
    const dirs = fs.readdirSync(rootPath, { withFileTypes: true })
      .filter(dirent => dirent.isDirectory())
      .map(dirent => dirent.name)
      .filter(name => !name.includes('Tests') && !name.includes('node_modules') && !name.includes('bin') && !name.includes('obj'));
    
    for (const dir of dirs) {
      const dirPath = path.join(rootPath, dir);
      try {
        const files = fs.readdirSync(dirPath);
        if (files.some(f => f.endsWith('.csproj') && !f.includes('Tests'))) {
          projectDir = dir;
          break;
        }
      } catch (e) {
        // Skip directories we can't read
      }
    }
  }
}

// Calculate absolute path from tests/e2e to project directory
const projectPath = path.resolve(__dirname, '..', '..', projectDir);

// Verify that the project directory exists
if (!fs.existsSync(projectPath)) {
  console.warn(`Warning: Project directory does not exist: ${projectPath}, using default: IcutechTestApi`);
  const defaultPath = path.resolve(__dirname, '..', '..', 'IcutechTestApi');
  if (fs.existsSync(defaultPath)) {
    projectDir = 'IcutechTestApi';
  } else {
    throw new Error(`Project directory does not exist: ${projectPath}. Please set PROJECT_DIR environment variable.`);
  }
}

// Final project path
const finalProjectPath = path.resolve(__dirname, '..', '..', projectDir);

// Check for .csproj file in the project directory
if (fs.existsSync(finalProjectPath)) {
  try {
    const files = fs.readdirSync(finalProjectPath);
    const csprojFiles = files.filter(f => f.endsWith('.csproj') && !f.includes('Tests'));
    if (csprojFiles.length === 0) {
      console.warn(`Warning: No .csproj file found in project directory: ${finalProjectPath}`);
    }
  } catch (e) {
    console.warn(`Warning: Could not read project directory: ${finalProjectPath}`);
  }
}

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
    command: `cd ${JSON.stringify(finalProjectPath)} && dotnet run`,
    url: 'http://localhost:5030',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});

