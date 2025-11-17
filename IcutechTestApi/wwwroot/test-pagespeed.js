#!/usr/bin/env node

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const API_URL = process.env.API_URL || 'http://localhost:5030';
const LIGHTHOUSE_MOBILE_THRESHOLD = 100;
const LIGHTHOUSE_DESKTOP_THRESHOLD = 90;

console.log('🚀 Starting PageSpeed/Lighthouse tests...\n');
console.log(`📍 Testing URL: ${API_URL}\n`);

function runLighthouse(url, preset = 'mobile') {
    try {
        console.log(`📱 Running Lighthouse (${preset})...`);
        
        const output = execSync(
            `lighthouse "${url}" --preset=${preset} --output=json --chrome-flags="--headless --no-sandbox" --quiet`,
            { encoding: 'utf-8', maxBuffer: 10 * 1024 * 1024 }
        );
        
        const report = JSON.parse(output);
        const scores = {
            performance: Math.round(report.categories.performance.score * 100),
            accessibility: Math.round(report.categories.accessibility.score * 100),
            bestPractices: Math.round(report.categories['best-practices'].score * 100),
            seo: Math.round(report.categories.seo.score * 100)
        };
        
        return { scores, report };
    } catch (error) {
        console.error(`❌ Error running Lighthouse (${preset}):`, error.message);
        return null;
    }
}

function checkRenderBlocking(resources) {
    const renderBlocking = resources.filter(r => 
        r.renderBlocking === true && 
        (r.mimeType?.includes('css') || r.mimeType?.includes('javascript'))
    );
    return renderBlocking;
}

async function main() {
    // Mobile test
    const mobileResult = runLighthouse(API_URL, 'mobile');
    if (!mobileResult) {
        console.error('❌ Mobile test failed');
        process.exit(1);
    }
    
    // Desktop test
    const desktopResult = runLighthouse(API_URL, 'desktop');
    if (!desktopResult) {
        console.error('❌ Desktop test failed');
        process.exit(1);
    }
    
    // Display results
    console.log('\n📊 Lighthouse Results:\n');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('📱 MOBILE SCORES:');
    console.log(`   Performance:     ${mobileResult.scores.performance}/100`);
    console.log(`   Accessibility:   ${mobileResult.scores.accessibility}/100`);
    console.log(`   Best Practices: ${mobileResult.scores.bestPractices}/100`);
    console.log(`   SEO:            ${mobileResult.scores.seo}/100`);
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('🖥️  DESKTOP SCORES:');
    console.log(`   Performance:     ${desktopResult.scores.performance}/100`);
    console.log(`   Accessibility:   ${desktopResult.scores.accessibility}/100`);
    console.log(`   Best Practices: ${desktopResult.scores.bestPractices}/100`);
    console.log(`   SEO:            ${desktopResult.scores.seo}/100`);
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');
    
    // Check render-blocking resources
    const mobileRenderBlocking = checkRenderBlocking(mobileResult.report.audits['render-blocking-resources']?.details?.items || []);
    const desktopRenderBlocking = checkRenderBlocking(desktopResult.report.audits['render-blocking-resources']?.details?.items || []);
    
    if (mobileRenderBlocking.length > 0 || desktopRenderBlocking.length > 0) {
        console.log('⚠️  Render-blocking resources detected:');
        if (mobileRenderBlocking.length > 0) {
            console.log('   Mobile:', mobileRenderBlocking.map(r => r.url).join(', '));
        }
        if (desktopRenderBlocking.length > 0) {
            console.log('   Desktop:', desktopRenderBlocking.map(r => r.url).join(', '));
        }
        console.log('');
    } else {
        console.log('✅ No render-blocking resources detected\n');
    }
    
    // Check thresholds
    const mobilePass = mobileResult.scores.performance >= LIGHTHOUSE_MOBILE_THRESHOLD;
    const desktopPass = desktopResult.scores.performance >= LIGHTHOUSE_DESKTOP_THRESHOLD;
    
    if (!mobilePass) {
        console.error(`❌ Mobile performance score ${mobileResult.scores.performance} is below threshold ${LIGHTHOUSE_MOBILE_THRESHOLD}`);
    }
    
    if (!desktopPass) {
        console.error(`❌ Desktop performance score ${desktopResult.scores.performance} is below threshold ${LIGHTHOUSE_DESKTOP_THRESHOLD}`);
    }
    
    if (mobilePass && desktopPass) {
        console.log('✅ All performance thresholds met!\n');
        process.exit(0);
    } else {
        console.error('\n❌ Performance thresholds not met. Build failed.\n');
        process.exit(1);
    }
}

main().catch(error => {
    console.error('❌ Test execution failed:', error);
    process.exit(1);
});

