# Инструкции по сборке Production версии

## Требования

- Node.js 16+ и npm
- (Опционально) Lighthouse CLI для тестирования производительности

## Быстрая сборка

### Вариант 1: Использование npm скриптов

```bash
cd IcutechTestApi/wwwroot
npm install
npm run build:prod
```

Это создаст:
- `styles.min.css` - минифицированный CSS
- `app.min.js` - минифицированный JS
- `index-prod.html` - production HTML с inline критическим CSS

### Вариант 2: Ручная сборка

```bash
cd IcutechTestApi/wwwroot

# Минификация CSS
npm run minify:css

# Минификация JS
npm run minify:js

# Создание production HTML
npm run build
```

## Оптимизации в Production версии

### 1. Минификация
- CSS: удалены комментарии, пробелы, оптимизированы селекторы
- JS: удалены комментарии, минифицирован код

### 2. Inline критический CSS
- Критические стили (для видимой части страницы) встроены в `<head>`
- Остальные стили загружаются асинхронно через `preload`

### 3. Отложенная загрузка
- JavaScript загружается с атрибутом `defer`
- CSS загружается асинхронно через `preload` с fallback для noscript

### 4. Мета-теги
- `viewport` для мобильных устройств
- `description` для SEO
- `theme-color` для браузеров

### 5. Кэширование
Настройте заголовки кэширования на сервере:

```nginx
# Nginx example
location ~* \.(css|js)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}

location ~* \.(html)$ {
    expires 1h;
    add_header Cache-Control "public, must-revalidate";
}
```

Или в ASP.NET Core (Program.cs):

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".min.css") || ctx.File.Name.EndsWith(".min.js"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
        else if (ctx.File.Name.EndsWith(".html"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600, must-revalidate");
        }
    }
});
```

## Тестирование производительности

### Lighthouse CLI

```bash
# Установка Lighthouse
npm install -g lighthouse

# Запуск теста
npm run test:lighthouse
```

### PageSpeed тесты

```bash
# Убедитесь, что API запущен на localhost:5030
npm run test:pagespeed
```

Тест проверит:
- Mobile score >= 100
- Desktop score >= 90
- Отсутствие render-blocking ресурсов

## Оптимизация изображений (если есть)

### Конвертация в WebP

```bash
# Установка cwebp
# macOS: brew install webp
# Ubuntu: sudo apt-get install webp
# Windows: choco install webp

# Конвертация
cwebp -q 80 input.jpg -o output.webp
```

### Конвертация в AVIF

```bash
# Установка avifenc
# macOS: brew install libavif
# Ubuntu: sudo apt-get install libavif-bin

# Конвертация
avifenc --min 0 --max 63 -a end-usage=q -a cq-level=30 input.jpg output.avif
```

### Batch конвертация

```bash
# WebP
for file in *.jpg *.png; do
    cwebp -q 80 "$file" -o "${file%.*}.webp"
done

# AVIF
for file in *.jpg *.png; do
    avifenc --min 0 --max 63 -a end-usage=q -a cq-level=30 "$file" "${file%.*}.avif"
done
```

### Использование в HTML

```html
<picture>
    <source srcset="image.avif" type="image/avif">
    <source srcset="image.webp" type="image/webp">
    <img src="image.jpg" alt="Description" width="600" height="400" loading="lazy">
</picture>
```

## Сжатие (Gzip/Brotli)

### Dockerfile (Nginx)

```dockerfile
FROM nginx:alpine

# Установка brotli
RUN apk add --no-cache brotli

# Конфигурация
COPY nginx.conf /etc/nginx/nginx.conf
COPY wwwroot /usr/share/nginx/html
```

### nginx.conf

```nginx
http {
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/json application/xml;

    brotli on;
    brotli_comp_level 6;
    brotli_types text/plain text/css text/xml text/javascript application/javascript application/json application/xml;
}
```

### ASP.NET Core

В `Program.cs` добавьте:

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/javascript", "text/css" }
    );
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

app.UseResponseCompression();
```

## Результаты оптимизации

После применения всех оптимизаций ожидаемые результаты:

- **First Contentful Paint (FCP)**: < 1.8s
- **Largest Contentful Paint (LCP)**: < 2.5s
- **Time to Interactive (TTI)**: < 3.8s
- **Total Blocking Time (TBT)**: < 200ms
- **Cumulative Layout Shift (CLS)**: < 0.1
- **Lighthouse Mobile Score**: 100
- **Lighthouse Desktop Score**: 90+

## Проверка

1. Откройте `index-prod.html` в браузере
2. Откройте DevTools → Network
3. Проверьте, что:
   - CSS загружается асинхронно
   - JS загружается с defer
   - Критический CSS встроен в HTML
4. Запустите Lighthouse:
   ```bash
   lighthouse http://localhost:5030/index-prod.html --view
   ```

