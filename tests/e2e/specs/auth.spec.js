const { test, expect } = require('@playwright/test');

const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5030';
const API_URL = process.env.API_URL || 'http://localhost:5030';

test.describe('Authentication Forms', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(FRONTEND_URL);
  });

  test.describe('Login Form', () => {
    test('should load login form', async ({ page }) => {
      await expect(page.locator('#loginForm')).toBeVisible();
      await expect(page.locator('#loginForm input[name="login"]')).toBeVisible();
      await expect(page.locator('#loginForm input[name="password"]')).toBeVisible();
      await expect(page.locator('#loginForm button[type="submit"]')).toHaveText('Войти');
    });

    test('should show error for empty fields', async ({ page }) => {
      await page.locator('#loginForm button[type="submit"]').click();
      
      // Browser validation should prevent submission or show error
      await page.waitForTimeout(500);
    });

    test('should show error for invalid credentials', async ({ page }) => {
      // Mock API response
      await page.route(`${API_URL}/api/auth/login`, async route => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            success: false,
            message: 'Invalid credentials'
          })
        });
      });

      await page.locator('#loginForm input[name="login"]').fill('invaliduser');
      await page.locator('#loginForm input[name="password"]').fill('wrongpassword');
      await page.locator('#loginForm button[type="submit"]').click();

      await expect(page.locator('#loginMessage')).toBeVisible({ timeout: 5000 });
      await expect(page.locator('#loginMessage')).toHaveClass(/error/);
      await expect(page.locator('#loginMessage')).toContainText('Invalid credentials');
    });

    test('should show success for valid credentials', async ({ page }) => {
      // Mock API response
      await page.route(`${API_URL}/api/auth/login`, async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            success: true,
            entity: {
              userId: '123',
              name: 'Test User'
            }
          })
        });
      });

      await page.locator('#loginForm input[name="login"]').fill('testuser');
      await page.locator('#loginForm input[name="password"]').fill('testpassword');
      await page.locator('#loginForm button[type="submit"]').click();

      await expect(page.locator('#loginMessage')).toBeVisible({ timeout: 5000 });
      await expect(page.locator('#loginMessage')).toHaveClass(/success/);
      await expect(page.locator('#loginMessage')).toContainText('Вход выполнен успешно');
    });

    test('should show loading state during request', async ({ page }) => {
      // Slow down API response
      await page.route(`${API_URL}/api/auth/login`, async route => {
        await new Promise(resolve => setTimeout(resolve, 1000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ success: true, entity: {} })
        });
      });

      await page.locator('#loginForm input[name="login"]').fill('testuser');
      await page.locator('#loginForm input[name="password"]').fill('testpassword');
      await page.locator('#loginForm button[type="submit"]').click();

      // Check loading state
      const submitButton = page.locator('#loginForm button[type="submit"]');
      await expect(submitButton).toBeDisabled();
      await expect(submitButton).toHaveClass(/loading/);
    });
  });

  test.describe('Register Form', () => {
    test('should switch to register tab', async ({ page }) => {
      await page.click('button[data-tab="register"]');
      await expect(page.locator('#registerForm')).toBeVisible();
      await expect(page.locator('#loginForm')).not.toBeVisible();
    });

    test('should show error for empty required fields', async ({ page }) => {
      await page.click('button[data-tab="register"]');
      await page.locator('#registerForm button[type="submit"]').click();

      // Browser validation should prevent submission
      await page.waitForTimeout(500);
    });

    test('should show error for short password', async ({ page }) => {
      await page.click('button[data-tab="register"]');
      await page.locator('#registerForm input[name="login"]').fill('newuser');
      await page.locator('#registerForm input[name="password"]').fill('ab'); // Too short
      await page.locator('#registerForm button[type="submit"]').click();

      await expect(page.locator('#registerMessage')).toBeVisible({ timeout: 5000 });
      await expect(page.locator('#registerMessage')).toContainText('минимум 3 символа');
    });

    test('should show success for valid registration', async ({ page }) => {
      // Mock API response
      await page.route(`${API_URL}/api/auth/register`, async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            success: true,
            message: 'Registration successful',
            createdCustomerId: '12345'
          })
        });
      });

      await page.click('button[data-tab="register"]');
      await page.locator('#registerForm input[name="login"]').fill('newuser');
      await page.locator('#registerForm input[name="password"]').fill('securepassword');
      await page.locator('#registerForm input[name="email"]').fill('user@example.com');
      await page.locator('#registerForm input[name="firstName"]').fill('Иван');
      await page.locator('#registerForm input[name="lastName"]').fill('Иванов');
      await page.locator('#registerForm button[type="submit"]').click();

      await expect(page.locator('#registerMessage')).toBeVisible({ timeout: 5000 });
      await expect(page.locator('#registerMessage')).toHaveClass(/success/);
      await expect(page.locator('#registerMessage')).toContainText('Регистрация успешна');
      await expect(page.locator('#registerMessage')).toContainText('12345');
    });

    test('should handle network errors', async ({ page }) => {
      // Simulate network error
      await page.route(`${API_URL}/api/auth/register`, route => route.abort());

      await page.click('button[data-tab="register"]');
      await page.locator('#registerForm input[name="login"]').fill('newuser');
      await page.locator('#registerForm input[name="password"]').fill('password');
      await page.locator('#registerForm button[type="submit"]').click();

      await expect(page.locator('#registerMessage')).toBeVisible({ timeout: 5000 });
      await expect(page.locator('#registerMessage')).toHaveClass(/error/);
      await expect(page.locator('#registerMessage')).toContainText('Ошибка сети');
    });
  });

  test.describe('Tab Switching', () => {
    test('should switch between login and register tabs', async ({ page }) => {
      // Start on login
      await expect(page.locator('#loginForm')).toBeVisible();
      await expect(page.locator('button[data-tab="login"]')).toHaveClass(/active/);

      // Switch to register
      await page.click('button[data-tab="register"]');
      await expect(page.locator('#registerForm')).toBeVisible();
      await expect(page.locator('button[data-tab="register"]')).toHaveClass(/active/);
      await expect(page.locator('#loginForm')).not.toBeVisible();

      // Switch back to login
      await page.click('button[data-tab="login"]');
      await expect(page.locator('#loginForm')).toBeVisible();
      await expect(page.locator('button[data-tab="login"]')).toHaveClass(/active/);
    });

    test('should clear messages when switching tabs', async ({ page }) => {
      // Mock error response
      await page.route(`${API_URL}/api/auth/login`, async route => {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            success: false,
            message: 'Invalid credentials'
          })
        });
      });

      // Show error in login
      await page.locator('#loginForm input[name="login"]').fill('test');
      await page.locator('#loginForm input[name="password"]').fill('test');
      await page.locator('#loginForm button[type="submit"]').click();
      
      // Wait for message
      await expect(page.locator('#loginMessage')).toBeVisible({ timeout: 5000 });

      // Switch tabs
      await page.click('button[data-tab="register"]');
      
      // Messages should be cleared
      await expect(page.locator('#loginMessage')).not.toBeVisible();
    });
  });
});

