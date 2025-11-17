const fs = require('fs');
const path = require('path');

// Минификация CSS (простая версия - удаление комментариев, пробелов)
function minifyCSS(css) {
    return css
        .replace(/\/\*[\s\S]*?\*\//g, '') // Удалить комментарии
        .replace(/\s+/g, ' ') // Заменить множественные пробелы на один
        .replace(/\s*([{}:;,])\s*/g, '$1') // Удалить пробелы вокруг символов
        .replace(/;\s*}/g, '}') // Удалить точку с запятой перед закрывающей скобкой
        .trim();
}

// Минификация JS (простая версия)
function minifyJS(js) {
    return js
        .replace(/\/\*[\s\S]*?\*\//g, '') // Удалить многострочные комментарии
        .replace(/\/\/.*/g, '') // Удалить однострочные комментарии
        .replace(/\s+/g, ' ') // Заменить множественные пробелы
        .replace(/\s*([{}();,=+\-*/%<>!&|?:])\s*/g, '$1') // Удалить пробелы вокруг операторов
        .trim();
}

// Извлечение критического CSS (первые стили для видимой части)
function extractCriticalCSS(css) {
    // Берем стили для body, container, card, card-header, tabs, form (первые ~150 строк)
    const criticalSelectors = [
        '*', 'body', '.container', '.card', '.card-header', 
        '.card-header h1', '.card-header .subtitle', '.tabs', 
        '.tab-button', '.form', '.form-group', '.form-group label', 
        '.form-group input', '.btn', '.btn-primary'
    ];
    
    const lines = css.split('\n');
    let critical = '';
    let inCriticalBlock = false;
    let braceCount = 0;
    
    for (let i = 0; i < Math.min(200, lines.length); i++) {
        const line = lines[i];
        const trimmed = line.trim();
        
        // Проверяем, начинается ли селектор с критического
        if (criticalSelectors.some(sel => trimmed.startsWith(sel) || trimmed.includes(sel + ' {'))) {
            inCriticalBlock = true;
            braceCount = (line.match(/{/g) || []).length - (line.match(/}/g) || []).length;
        }
        
        if (inCriticalBlock) {
            critical += line + '\n';
            braceCount += (line.match(/{/g) || []).length - (line.match(/}/g) || []).length;
            if (braceCount <= 0 && trimmed.includes('}')) {
                inCriticalBlock = false;
            }
        }
    }
    
    return minifyCSS(critical || css.substring(0, 3000));
}

// Чтение файлов
const wwwrootPath = path.join(__dirname);
const cssPath = path.join(wwwrootPath, 'styles.css');
const jsPath = path.join(wwwrootPath, 'app.js');
const htmlPath = path.join(wwwrootPath, 'index.html');

const css = fs.readFileSync(cssPath, 'utf8');
const js = fs.readFileSync(jsPath, 'utf8');
const html = fs.readFileSync(htmlPath, 'utf8');

// Минификация
const minifiedCSS = minifyCSS(css);
const minifiedJS = minifyJS(js);
const criticalCSS = extractCriticalCSS(css);

// Сохранение минифицированных файлов
fs.writeFileSync(path.join(wwwrootPath, 'styles.min.css'), minifiedCSS);
fs.writeFileSync(path.join(wwwrootPath, 'app.min.js'), minifiedJS);

// Создание production HTML
const productionHTML = html
    .replace(
        '<link rel="stylesheet" href="styles.css">',
        `<style>${criticalCSS}</style>\n    <link rel="preload" href="styles.min.css" as="style" onload="this.onload=null;this.rel='stylesheet'">\n    <noscript><link rel="stylesheet" href="styles.min.css"></noscript>`
    )
    .replace(
        '<script src="app.js"></script>',
        '<script defer src="app.min.js"></script>'
    )
    .replace(
        '<meta name="viewport" content="width=device-width, initial-scale=1.0">',
        `<meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="Icutech Test API - Система авторизации">
    <meta name="theme-color" content="#6366f1">
    <link rel="preconnect" href="${process.env.API_URL || ''}">`
    );

fs.writeFileSync(path.join(wwwrootPath, 'index-prod.html'), productionHTML);

console.log('✅ Production build completed!');
console.log('📦 Generated files:');
console.log('   - styles.min.css');
console.log('   - app.min.js');
console.log('   - index-prod.html');

