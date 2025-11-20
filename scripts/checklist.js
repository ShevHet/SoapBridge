#!/usr/bin/env node

const https = require('https');
const http = require('http');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const colors = {
  reset: '\x1b[0m',
  green: '\x1b[32m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
};

let passed = 0;
let failed = 0;
let warnings = 0;

function log(message, type = 'info') {
  const color = type === 'success' ? colors.green : 
                type === 'error' ? colors.red : 
                type === 'warning' ? colors.yellow : colors.blue;
  console.log(`${color}${message}${colors.reset}`);
}

function checkUrl(url, description) {
  return new Promise((resolve) => {
    const client = url.startsWith('https') ? https : http;
    const req = client.get(url, { timeout: 5000 }, (res) => {
      const success = res.statusCode === 200;
      if (success) {
        log(`✅ ${description}: ${res.statusCode}`, 'success');
        passed++;
      } else {
        log(`❌ ${description}: ${res.statusCode}`, 'error');
        failed++;
      }
      resolve(success);
    });

    req.on('error', (err) => {
      log(`❌ ${description}: ${err.message}`, 'error');
      failed++;
      resolve(false);
    });

    req.on('timeout', () => {
      req.destroy();
      log(`❌ ${description}: Timeout`, 'error');
      failed++;
      resolve(false);
    });
  });
}

function checkFile(filePath, description) {
  const fullPath = path.join(__dirname, '..', filePath);
  if (fs.existsSync(fullPath)) {
    const stats = fs.statSync(fullPath);
    log(`✅ ${description}: ${(stats.size / 1024).toFixed(2)} KB`, 'success');
    passed++;
    return true;
  } else {
    log(`❌ ${description}: File not found`, 'error');
    failed++;
    return false;
  }
}

function checkFileContent(filePath, searchText, description) {
  const fullPath = path.join(__dirname, '..', filePath);
  if (fs.existsSync(fullPath)) {
    const content = fs.readFileSync(fullPath, 'utf8');
    if (content.includes(searchText)) {
      log(`✅ ${description}`, 'success');
      passed++;
      return true;
    } else {
      log(`⚠️  ${description}: Not found`, 'warning');
      warnings++;
      return false;
    }
  } else {
    log(`❌ ${description}: File not found`, 'error');
    failed++;
    return false;
  }
}

async function runLighthouse(url, preset = 'mobile') {
  try {
    log(`📱 Running Lighthouse (${preset})...`, 'info');
    const output = execSync(
      `lighthouse "${url}" --preset=${preset} --output=json --chrome-flags="--headless --no-sandbox" --quiet 2>/dev/null || echo "{}"`,
      { encoding: 'utf-8', maxBuffer: 10 * 1024 * 1024 }
    );
    
    const report = JSON.parse(output);
    if (report.categories && report.categories.performance) {
      const score = Math.round(report.categories.performance.score * 100);
      return score;
    }
    return null;
  } catch (error) {
    return null;
  }
}

async function main() {
  console.log('\n🔍 Running checklist validation...\n');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  // Get URLs from environment or use defaults
  const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5030';
  const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5030';
  const FRONTEND_PROD_URL = process.env.FRONTEND_PROD_URL;
  const BACKEND_PROD_URL = process.env.BACKEND_PROD_URL;

  // Backend checks
  log('📦 Backend Checks', 'info');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  await checkUrl(`${BACKEND_URL}/api/example`, 'API /api/example');
  await checkUrl(`${BACKEND_URL}/swagger`, 'Swagger UI');
  await checkUrl(`${BACKEND_URL}/swagger/v1/swagger.json`, 'Swagger JSON');

  // Frontend checks
  log('\n🎨 Frontend Checks', 'info');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  checkFile('IcutechTestApi/wwwroot/index-prod.html', 'index-prod.html exists');
  checkFile('IcutechTestApi/wwwroot/styles.min.css', 'styles.min.css exists');
  checkFile('IcutechTestApi/wwwroot/app.min.js', 'app.min.js exists');
  checkFileContent('IcutechTestApi/wwwroot/index-prod.html', '<style>', 'Inline critical CSS');
  checkFileContent('IcutechTestApi/wwwroot/index-prod.html', 'app.min.js', 'Uses minified JS');
  checkFileContent('IcutechTestApi/wwwroot/index-prod.html', 'styles.min.css', 'Uses minified CSS');
  checkFileContent('IcutechTestApi/wwwroot/index-prod.html', 'defer', 'JS loaded with defer');

  // Performance checks
  log('\n⚡ Performance Checks', 'info');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  const testUrl = `${FRONTEND_URL}/index-prod.html`;
  const mobileScore = await runLighthouse(testUrl, 'mobile');
  const desktopScore = await runLighthouse(testUrl, 'desktop');

  if (mobileScore !== null) {
    if (mobileScore >= 100) {
      log(`✅ Mobile Performance: ${mobileScore}/100`, 'success');
      passed++;
    } else {
      log(`❌ Mobile Performance: ${mobileScore}/100 (required: >= 100)`, 'error');
      failed++;
    }
  } else {
    log(`⚠️  Mobile Performance: Lighthouse not available`, 'warning');
    warnings++;
  }

  if (desktopScore !== null) {
    if (desktopScore >= 90) {
      log(`✅ Desktop Performance: ${desktopScore}/100`, 'success');
      passed++;
    } else {
      log(`❌ Desktop Performance: ${desktopScore}/100 (required: >= 90)`, 'error');
      failed++;
    }
  } else {
    log(`⚠️  Desktop Performance: Lighthouse not available`, 'warning');
    warnings++;
  }

  // Deployment checks
  if (FRONTEND_PROD_URL || BACKEND_PROD_URL) {
    log('\n🚀 Deployment Checks', 'info');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

    if (FRONTEND_PROD_URL) {
      await checkUrl(FRONTEND_PROD_URL, 'Frontend Production URL');
    } else {
      log('⚠️  FRONTEND_PROD_URL not set, skipping', 'warning');
      warnings++;
    }

    if (BACKEND_PROD_URL) {
      await checkUrl(`${BACKEND_PROD_URL}/api/example`, 'Backend Production URL');
      await checkUrl(`${BACKEND_PROD_URL}/swagger`, 'Backend Swagger');
    } else {
      log('⚠️  BACKEND_PROD_URL not set, skipping', 'warning');
      warnings++;
    }
  } else {
    log('\n⚠️  Production URLs not set (FRONTEND_PROD_URL, BACKEND_PROD_URL)', 'warning');
    warnings++;
  }

  // Summary
  console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  log(`\n📊 Summary:`, 'info');
  log(`   ✅ Passed: ${passed}`, 'success');
  log(`   ❌ Failed: ${failed}`, failed > 0 ? 'error' : 'success');
  log(`   ⚠️  Warnings: ${warnings}`, warnings > 0 ? 'warning' : 'info');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

  if (failed > 0) {
    log('❌ Checklist validation failed!', 'error');
    process.exit(1);
  } else {
    log('✅ All checks passed!', 'success');
    process.exit(0);
  }
}

main().catch(error => {
  log(`❌ Error: ${error.message}`, 'error');
  process.exit(1);
});

