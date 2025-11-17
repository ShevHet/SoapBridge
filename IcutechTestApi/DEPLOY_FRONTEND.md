# Деплой фронтенда на Netlify/Vercel

## Вариант 1: Netlify

### Способ 1: Drag & Drop (быстрый)

1. **Подготовка файлов:**
   ```bash
   cd IcutechTestApi/wwwroot
   npm run build:prod
   ```
   
   Создайте папку `netlify-deploy` со следующими файлами:
   - `index.html` (переименуйте `index-prod.html`)
   - `styles.min.css`
   - `app.min.js`

2. **Деплой через UI:**
   - Зайдите на [netlify.com](https://www.netlify.com)
   - Зарегистрируйтесь/войдите
   - Перетащите папку `netlify-deploy` в область "Deploy manually"
   - Дождитесь завершения деплоя
   - Получите URL вида: `https://random-name-123.netlify.app`

3. **Настройка домена (опционально):**
   - Site settings → Domain management
   - Добавьте кастомный домен

### Способ 2: Git интеграция (рекомендуется)

1. **Создайте `netlify.toml` в корне проекта:**

```toml
[build]
  publish = "IcutechTestApi/wwwroot"
  command = "cd IcutechTestApi/wwwroot && npm install && npm run build:prod && cp index-prod.html index.html"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200

[[headers]]
  for = "/*.min.css"
  [headers.values]
    Cache-Control = "public, max-age=31536000, immutable"
    Content-Type = "text/css; charset=utf-8"

[[headers]]
  for = "/*.min.js"
  [headers.values]
    Cache-Control = "public, max-age=31536000, immutable"
    Content-Type = "application/javascript; charset=utf-8"

[[headers]]
  for = "/*.html"
  [headers.values]
    Cache-Control = "public, max-age=3600, must-revalidate"
    Content-Type = "text/html; charset=utf-8"

[[headers]]
  for = "/*"
  [headers.values]
    X-Content-Type-Options = "nosniff"
    X-Frame-Options = "DENY"
    X-XSS-Protection = "1; mode=block"
```

2. **Подключение репозитория:**
   - Netlify Dashboard → Add new site → Import an existing project
   - Выберите GitHub/GitLab/Bitbucket
   - Выберите репозиторий
   - Настройки:
     - **Build command:** `cd IcutechTestApi/wwwroot && npm install && npm run build:prod && cp index-prod.html index.html`
     - **Publish directory:** `IcutechTestApi/wwwroot`
   - Deploy site

3. **Настройка переменных окружения (если нужно):**
   - Site settings → Environment variables
   - Добавьте `API_URL` если фронтенд обращается к внешнему API

### Способ 3: Netlify CLI

```bash
# Установка CLI
npm install -g netlify-cli

# Логин
netlify login

# Деплой
cd IcutechTestApi/wwwroot
npm run build:prod
cp index-prod.html index.html
netlify deploy --prod --dir=.
```

## Вариант 2: Vercel

### Способ 1: Drag & Drop

1. **Подготовка:**
   ```bash
   cd IcutechTestApi/wwwroot
   npm run build:prod
   cp index-prod.html index.html
   ```

2. **Деплой:**
   - Зайдите на [vercel.com](https://vercel.com)
   - Зарегистрируйтесь/войдите
   - New Project → Import → Drag & Drop
   - Загрузите папку `wwwroot`
   - Deploy

### Способ 2: Git интеграция

1. **Создайте `vercel.json` в `wwwroot`:**

```json
{
  "version": 2,
  "buildCommand": "npm install && npm run build:prod && cp index-prod.html index.html",
  "outputDirectory": ".",
  "headers": [
    {
      "source": "/(.*\\.min\\.(css|js))",
      "headers": [
        {
          "key": "Cache-Control",
          "value": "public, max-age=31536000, immutable"
        }
      ]
    },
    {
      "source": "/(.*\\.html)",
      "headers": [
        {
          "key": "Cache-Control",
          "value": "public, max-age=3600, must-revalidate"
        }
      ]
    }
  ],
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

2. **Подключение:**
   - Vercel Dashboard → Add New Project
   - Import Git Repository
   - Выберите репозиторий
   - Root Directory: `IcutechTestApi/wwwroot`
   - Deploy

### Способ 3: Vercel CLI

```bash
# Установка
npm install -g vercel

# Деплой
cd IcutechTestApi/wwwroot
npm run build:prod
cp index-prod.html index.html
vercel --prod
```

## Проверка деплоя

После деплоя проверьте:

1. **Доступность сайта:**
   ```bash
   curl -I https://your-site.netlify.app
   # Должен вернуть 200 OK
   ```

2. **Кэширование:**
   ```bash
   curl -I https://your-site.netlify.app/styles.min.css
   # Должен вернуть Cache-Control: public, max-age=31536000, immutable
   ```

3. **Сжатие:**
   ```bash
   curl -H "Accept-Encoding: gzip" -I https://your-site.netlify.app/styles.min.css
   # Должен вернуть Content-Encoding: gzip или br
   ```

4. **Lighthouse:**
   ```bash
   lighthouse https://your-site.netlify.app --view
   ```

## Настройка CORS для API

Если фронтенд на Netlify/Vercel, а API на другом домене, настройте CORS в `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-site.netlify.app", "https://your-site.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// В pipeline:
app.UseCors("AllowFrontend");
```

## Обновление API URL

В `app.js` обновите `API_BASE_URL`:

```javascript
// Для production
const API_BASE_URL = process.env.API_URL || 'https://your-api-url.com';
```

Или используйте переменные окружения на Netlify/Vercel.

