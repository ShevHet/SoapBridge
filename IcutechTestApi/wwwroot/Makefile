.PHONY: build minify test clean install

# Установка зависимостей
install:
	npm install

# Минификация CSS
minify-css:
	@echo "📦 Minifying CSS..."
	@node -e "const fs=require('fs');const css=fs.readFileSync('styles.css','utf8');fs.writeFileSync('styles.min.css',css.replace(/\/\*[\s\S]*?\*\//g,'').replace(/\s+/g,' ').replace(/\s*([{}:;,])\s*/g,'$$1').replace(/;\s*}/g,'}').trim());"
	@echo "✅ CSS minified"

# Минификация JS
minify-js:
	@echo "📦 Minifying JS..."
	@node -e "const fs=require('fs');const js=fs.readFileSync('app.js','utf8');fs.writeFileSync('app.min.js',js.replace(/\/\*[\s\S]*?\*\//g,'').replace(/\/\/.*/g,'').replace(/\s+/g,' ').replace(/\s*([{}();,=+\-*/%<>!&|?:])\s*/g,'$$1').trim());"
	@echo "✅ JS minified"

# Полная сборка
build: minify-css minify-js
	@echo "🚀 Building production version..."
	@node build.js
	@echo "✅ Build complete!"

# Тест производительности
test: build
	@echo "🧪 Running Lighthouse tests..."
	@npm run test:pagespeed || (echo "❌ Tests failed" && exit 1)

# Lighthouse тест (только)
test-lighthouse:
	@echo "🧪 Running Lighthouse..."
	@npm run test:lighthouse

# Очистка
clean:
	@echo "🧹 Cleaning build files..."
	@rm -f styles.min.css app.min.js
	@echo "✅ Clean complete"

# Все: установка, сборка, тесты
all: install build test
	@echo "✅ All done!"

