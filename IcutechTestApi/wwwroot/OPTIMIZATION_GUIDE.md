# Руководство по оптимизации ресурсов

## Оптимизация изображений

### Требования к изображениям

- **Размер файла**: < 100KB на изображение
- **Форматы**: WebP (основной), AVIF (современные браузеры), JPEG/PNG (fallback)
- **Размеры**: Указаны width и height для предотвращения layout shift

### Инструменты для конвертации

#### ImageMagick (универсальный)

```bash
# Установка
# macOS: brew install imagemagick
# Ubuntu: sudo apt-get install imagemagick
# Windows: choco install imagemagick

# Конвертация в WebP
convert input.jpg -quality 80 output.webp

# Конвертация в AVIF
convert input.jpg -quality 80 output.avif

# Оптимизация размера
convert input.jpg -strip -quality 85 -resize 1200x1200> output.jpg
```

#### cwebp (специализированный для WebP)

```bash
# Установка
# macOS: brew install webp
# Ubuntu: sudo apt-get install webp

# Базовая конвертация
cwebp -q 80 input.jpg -o output.webp

# С lossless
cwebp -lossless input.png -o output.webp

# С указанием размера
cwebp -q 80 -resize 1200 800 input.jpg -o output.webp
```

#### avifenc (для AVIF)

```bash
# Установка
# macOS: brew install libavif
# Ubuntu: sudo apt-get install libavif-bin

# Конвертация
avifenc --min 0 --max 63 -a end-usage=q -a cq-level=30 input.jpg output.avif

# С lossless
avifenc --lossless input.png output.avif
```

### Batch скрипты

#### convert-all.sh (WebP + AVIF)

```bash
#!/bin/bash

for file in *.jpg *.jpeg *.png; do
    if [ -f "$file" ]; then
        filename="${file%.*}"
        echo "Converting $file..."
        
        # WebP
        cwebp -q 80 "$file" -o "${filename}.webp"
        
        # AVIF
        avifenc --min 0 --max 63 -a end-usage=q -a cq-level=30 "$file" "${filename}.avif"
        
        # Оптимизация оригинала
        convert "$file" -strip -quality 85 "${filename}_opt.jpg"
    fi
done
```

#### optimize-images.js (Node.js)

```javascript
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

function optimizeImage(inputPath) {
    const ext = path.extname(inputPath);
    const name = path.basename(inputPath, ext);
    const dir = path.dirname(inputPath);
    
    console.log(`Optimizing ${inputPath}...`);
    
    // WebP
    try {
        execSync(`cwebp -q 80 "${inputPath}" -o "${dir}/${name}.webp"`);
        console.log(`  ✓ Created ${name}.webp`);
    } catch (e) {
        console.error(`  ✗ Failed to create WebP: ${e.message}`);
    }
    
    // AVIF
    try {
        execSync(`avifenc --min 0 --max 63 -a end-usage=q -a cq-level=30 "${inputPath}" "${dir}/${name}.avif"`);
        console.log(`  ✓ Created ${name}.avif`);
    } catch (e) {
        console.error(`  ✗ Failed to create AVIF: ${e.message}`);
    }
}

// Обработка всех изображений
const imagesDir = path.join(__dirname, 'images');
if (fs.existsSync(imagesDir)) {
    const files = fs.readdirSync(imagesDir)
        .filter(f => /\.(jpg|jpeg|png)$/i.test(f));
    
    files.forEach(file => {
        optimizeImage(path.join(imagesDir, file));
    });
}
```

### Использование в HTML

```html
<picture>
    <source srcset="image.avif" type="image/avif">
    <source srcset="image.webp" type="image/webp">
    <img 
        src="image.jpg" 
        alt="Описание изображения" 
        width="600" 
        height="400" 
        loading="lazy"
        decoding="async"
    >
</picture>
```

### Responsive изображения

```html
<picture>
    <source 
        media="(max-width: 640px)"
        srcset="image-mobile.avif 1x, image-mobile@2x.avif 2x"
        type="image/avif"
    >
    <source 
        media="(max-width: 640px)"
        srcset="image-mobile.webp 1x, image-mobile@2x.webp 2x"
        type="image/webp"
    >
    <source 
        srcset="image.avif 1x, image@2x.avif 2x"
        type="image/avif"
    >
    <source 
        srcset="image.webp 1x, image@2x.webp 2x"
        type="image/webp"
    >
    <img 
        src="image.jpg" 
        srcset="image.jpg 1x, image@2x.jpg 2x"
        alt="Описание"
        width="1200"
        height="800"
        loading="lazy"
    >
</picture>
```

## Preconnect/Preload для CDN

Если используются внешние ресурсы (CDN, шрифты, API):

```html
<head>
    <!-- Preconnect к внешним доменам -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    
    <!-- Preload критических ресурсов -->
    <link rel="preload" href="/fonts/main.woff2" as="font" type="font/woff2" crossorigin>
    <link rel="preload" href="/api/config" as="fetch" crossorigin>
    
    <!-- DNS prefetch для внешних ресурсов -->
    <link rel="dns-prefetch" href="https://api.example.com">
</head>
```

## Сжатие (Gzip/Brotli)

### Dockerfile с Nginx

```dockerfile
FROM nginx:alpine

# Установка brotli
RUN apk add --no-cache brotli nginx-mod-http-brotli

# Копирование конфигурации
COPY nginx.conf /etc/nginx/nginx.conf
COPY wwwroot /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### nginx.conf

```nginx
user nginx;
worker_processes auto;

events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_comp_level 6;
    gzip_types 
        text/plain 
        text/css 
        text/xml 
        text/javascript 
        application/javascript 
        application/json 
        application/xml 
        application/xhtml+xml 
        image/svg+xml;

    # Brotli compression
    brotli on;
    brotli_comp_level 6;
    brotli_types 
        text/plain 
        text/css 
        text/xml 
        text/javascript 
        application/javascript 
        application/json 
        application/xml 
        application/xhtml+xml 
        image/svg+xml;

    # Кэширование
    map $sent_http_content_type $expires {
        default                    off;
        text/html                  epoch;
        text/css                   max;
        application/javascript     max;
        ~image/                    1y;
        ~font/                     1y;
    }

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        expires $expires;

        location / {
            try_files $uri $uri/ /index.html;
        }

        # Кэширование статических файлов
        location ~* \.(css|js|jpg|jpeg|png|gif|ico|svg|webp|avif|woff|woff2)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

### ASP.NET Core (Program.cs)

```csharp
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { 
            "application/javascript", 
            "text/css",
            "application/json",
            "text/html"
        }
    );
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

var app = builder.Build();

app.UseResponseCompression();

// Static files with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLower();
        if (path.EndsWith(".min.css") || path.EndsWith(".min.js"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
        else if (path.EndsWith(".html"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600, must-revalidate");
        }
        else if (path.EndsWith(".webp") || path.EndsWith(".avif") || path.EndsWith(".jpg") || path.EndsWith(".png"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.Run();
```

## Проверка оптимизаций

### Проверка размеров изображений

```bash
# Найти все изображения > 100KB
find . -type f \( -name "*.jpg" -o -name "*.png" -o -name "*.webp" \) -size +100k

# Показать размеры всех изображений
find . -type f \( -name "*.jpg" -o -name "*.png" -o -name "*.webp" -o -name "*.avif" \) -exec ls -lh {} \;
```

### Проверка сжатия

```bash
# Проверка gzip
curl -H "Accept-Encoding: gzip" -I http://localhost:5030/styles.min.css

# Проверка brotli
curl -H "Accept-Encoding: br" -I http://localhost:5030/styles.min.css
```

### Lighthouse проверка

```bash
lighthouse http://localhost:5030/index-prod.html --view --only-categories=performance
```

## Чеклист оптимизации

- [ ] Все изображения < 100KB
- [ ] Изображения конвертированы в WebP/AVIF
- [ ] Указаны width и height для изображений
- [ ] Используется lazy loading для изображений
- [ ] Настроено gzip/brotli сжатие
- [ ] Настроено кэширование статических файлов
- [ ] Добавлены preconnect/preload для внешних ресурсов
- [ ] CSS минифицирован и критический CSS inline
- [ ] JS минифицирован и загружается с defer
- [ ] Lighthouse Mobile score >= 100
- [ ] Lighthouse Desktop score >= 90
- [ ] Нет render-blocking ресурсов

