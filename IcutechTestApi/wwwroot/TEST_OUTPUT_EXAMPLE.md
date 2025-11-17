# Пример вывода тестов производительности

## Запуск теста

```bash
cd IcutechTestApi/wwwroot
npm run test:pagespeed
```

## Пример успешного вывода

```
🚀 Starting PageSpeed/Lighthouse tests...

📍 Testing URL: http://localhost:5030

📱 Running Lighthouse (mobile)...
🖥️  Running Lighthouse (desktop)...

📊 Lighthouse Results:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📱 MOBILE SCORES:
   Performance:     100/100
   Accessibility:   100/100
   Best Practices:  100/100
   SEO:            100/100
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🖥️  DESKTOP SCORES:
   Performance:     100/100
   Accessibility:   100/100
   Best Practices:  100/100
   SEO:            100/100
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ No render-blocking resources detected

✅ All performance thresholds met!
```

## Пример вывода с ошибками

```
🚀 Starting PageSpeed/Lighthouse tests...

📍 Testing URL: http://localhost:5030

📱 Running Lighthouse (mobile)...
🖥️  Running Lighthouse (desktop)...

📊 Lighthouse Results:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📱 MOBILE SCORES:
   Performance:     85/100
   Accessibility:   100/100
   Best Practices:  100/100
   SEO:            100/100
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🖥️  DESKTOP SCORES:
   Performance:     88/100
   Accessibility:   100/100
   Best Practices:  100/100
   SEO:            100/100
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚠️  Render-blocking resources detected:
   Mobile: http://localhost:5030/styles.css
   Desktop: http://localhost:5030/styles.css

❌ Mobile performance score 85 is below threshold 100
❌ Desktop performance score 88 is below threshold 90

❌ Performance thresholds not met. Build failed.
```

## Детальный отчет Lighthouse

После запуска теста также создается HTML отчет:

```bash
lighthouse http://localhost:5030 --output=html --output-path=./lighthouse-report.html
```

Отчет содержит:
- Детальные метрики производительности
- Рекомендации по оптимизации
- Визуализацию загрузки ресурсов
- Анализ доступности и SEO

## Метрики производительности

### Core Web Vitals

- **LCP (Largest Contentful Paint)**: < 2.5s
- **FID (First Input Delay)**: < 100ms
- **CLS (Cumulative Layout Shift)**: < 0.1

### Дополнительные метрики

- **FCP (First Contentful Paint)**: < 1.8s
- **TTI (Time to Interactive)**: < 3.8s
- **TBT (Total Blocking Time)**: < 200ms
- **Speed Index**: < 3.4s

## Интерпретация результатов

### Performance Score

- **90-100**: Отлично ✅
- **50-89**: Требует улучшения ⚠️
- **0-49**: Плохо ❌

### Рекомендации при низком score

1. **Уменьшить размер JavaScript**
   - Минификация
   - Tree shaking
   - Code splitting

2. **Оптимизировать изображения**
   - Конвертация в WebP/AVIF
   - Lazy loading
   - Responsive images

3. **Устранить render-blocking ресурсы**
   - Inline критический CSS
   - Defer JavaScript
   - Preload важные ресурсы

4. **Улучшить кэширование**
   - Настроить Cache-Control заголовки
   - Использовать Service Workers

5. **Включить сжатие**
   - Gzip/Brotli
   - Минификация CSS/JS

