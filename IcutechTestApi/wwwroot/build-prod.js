#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

console.log('🔨 Building production HTML...');

const indexHtml = fs.readFileSync(path.join(__dirname, 'index.html'), 'utf8');
const styles = fs.readFileSync(path.join(__dirname, 'styles.css'), 'utf8');

const criticalCSS = styles;

let prodHtml = indexHtml;
prodHtml = prodHtml.replace(
  /<link rel="stylesheet" href="styles\.css">/,
  `<style>${criticalCSS}</style>\n    <link rel="preload" href="styles.min.css" as="style" onload="this.onload=null;this.rel='stylesheet'">\n    <noscript><link rel="stylesheet" href="styles.min.css"></noscript>`
);

prodHtml = prodHtml.replace(
  /<link href="https:\/\/cdn\.jsdelivr\.net\/npm\/bootstrap@5\.3\.0\/dist\/css\/bootstrap\.min\.css" rel="stylesheet">/,
  `<link rel="preconnect" href="https://cdn.jsdelivr.net" crossorigin>\n    <link rel="preload" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" as="style" onload="this.onload=null;this.rel='stylesheet'">\n    <noscript><link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet"></noscript>`
);

prodHtml = prodHtml.replace(
  /<script src="app\.js"><\/script>/,
  '<script src="app.min.js" defer></script>'
);

prodHtml = prodHtml.replace(
  /<script src="https:\/\/cdn\.jsdelivr\.net\/npm\/bootstrap@5\.3\.0\/dist\/js\/bootstrap\.bundle\.min\.js"><\/script>/,
  '<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" defer></script>'
);

prodHtml = prodHtml.replace(
  '</head>',
  `    <!-- Performance optimizations -->
    <link rel="dns-prefetch" href="https://cdn.jsdelivr.net">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="description" content="Icutech Test API - Authorization and Registration">
    <meta name="theme-color" content="#667eea">
</head>`
);

fs.writeFileSync(path.join(__dirname, 'index-prod.html'), prodHtml);

console.log('✅ Production HTML created: index-prod.html');
console.log('📦 Build complete!');

