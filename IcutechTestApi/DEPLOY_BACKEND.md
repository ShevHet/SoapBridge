# Деплой Backend (Docker)

## Вариант 1: Railway (Free Tier)

### Шаги деплоя

1. **Регистрация:**
   - Зайдите на [railway.app](https://railway.app)
   - Войдите через GitHub

2. **Создание проекта:**
   - New Project → Deploy from GitHub repo
   - Выберите репозиторий
   - Railway автоматически определит Dockerfile

3. **Настройка переменных окружения:**
   - Variables → Add Variable
   - Добавьте:
     ```
     ASPNETCORE_ENVIRONMENT=Production
     ASPNETCORE_URLS=http://+:8080
     SoapService__Url=http://isapi.mekashron.com/icu-tech/icutech-test.dll
     ```

4. **Настройка порта:**
   - Settings → Generate Domain
   - Railway автоматически назначит порт

5. **Деплой:**
   - Railway автоматически запустит `docker build`
   - Дождитесь завершения
   - Получите URL вида: `https://your-app.up.railway.app`

### Railway.json (опционально)

Создайте `railway.json` в корне проекта:

```json
{
  "$schema": "https://railway.app/railway.schema.json",
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "IcutechTestApi/Dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet IcutechTestApi.dll",
    "restartPolicyType": "ON_FAILURE",
    "restartPolicyMaxRetries": 10
  }
}
```

## Вариант 2: Render (Free Tier)

### Шаги деплоя

1. **Регистрация:**
   - Зайдите на [render.com](https://render.com)
   - Войдите через GitHub

2. **Создание Web Service:**
   - New → Web Service
   - Connect repository
   - Настройки:
     - **Name:** icutech-test-api
     - **Environment:** Docker
     - **Dockerfile Path:** `IcutechTestApi/Dockerfile`
     - **Docker Context:** `IcutechTestApi`
     - **Build Command:** (оставьте пустым, Dockerfile сам соберет)
     - **Start Command:** `dotnet IcutechTestApi.dll`

3. **Переменные окружения:**
   - Environment → Add Environment Variable
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   SoapService__Url=http://isapi.mekashron.com/icu-tech/icutech-test.dll
   ```

4. **Деплой:**
   - Create Web Service
   - Render автоматически соберет и задеплоит
   - Получите URL: `https://icutech-test-api.onrender.com`

### render.yaml (опционально)

Создайте `render.yaml` в корне:

```yaml
services:
  - type: web
    name: icutech-test-api
    env: docker
    dockerfilePath: ./IcutechTestApi/Dockerfile
    dockerContext: ./IcutechTestApi
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ASPNETCORE_URLS
        value: http://+:8080
      - key: SoapService__Url
        value: http://isapi.mekashron.com/icu-tech/icutech-test.dll
```

## Вариант 3: ngrok (Локальный туннель)

### Установка

```bash
# macOS
brew install ngrok

# Windows (Chocolatey)
choco install ngrok

# Linux
# Скачайте с https://ngrok.com/download
```

### Настройка

1. **Регистрация:**
   - Зайдите на [ngrok.com](https://ngrok.com)
   - Зарегистрируйтесь и получите authtoken

2. **Авторизация:**
   ```bash
   ngrok config add-authtoken YOUR_AUTH_TOKEN
   ```

3. **Запуск API локально:**
   ```bash
   cd IcutechTestApi
   dotnet run
   # API запустится на http://localhost:5030
   ```

4. **Создание туннеля:**
   ```bash
   # В другом терминале
   ngrok http 5030
   ```

5. **Получение URL:**
   ```
   Forwarding: https://abc123.ngrok.io -> http://localhost:5030
   ```

### Постоянный URL (платно)

Для бесплатного постоянного URL используйте ngrok с конфигурацией:

```yaml
# ngrok.yml
version: "2"
authtoken: YOUR_AUTH_TOKEN
tunnels:
  api:
    addr: 5030
    proto: http
```

Запуск:
```bash
ngrok start api
```

## Вариант 4: GitHub Actions (Автоматический деплой)

### Создайте `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Railway

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Build Docker image
        run: |
          docker build -t icutech-api:latest -f IcutechTestApi/Dockerfile IcutechTestApi/
      
      - name: Deploy to Railway
        uses: bervProject/railway-deploy@v0.2.1
        with:
          railway_token: ${{ secrets.RAILWAY_TOKEN }}
          service: icutech-test-api
          docker_image: icutech-api:latest
```

### Для Render:

```yaml
name: Deploy to Render

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy to Render
        uses: johnbeynon/render-deploy-action@v0.0.8
        with:
          service-id: ${{ secrets.RENDER_SERVICE_ID }}
          api-key: ${{ secrets.RENDER_API_KEY }}
```

## Обновление Dockerfile для production

Убедитесь, что `Dockerfile` настроен правильно:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Копируем только опубликованные файлы
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "IcutechTestApi.dll"]
```

## Проверка деплоя

```bash
# Проверка доступности
curl https://your-api-url.com/api/auth/login

# Проверка Swagger
curl https://your-api-url.com/swagger

# Проверка health
curl https://your-api-url.com/health
```

## Настройка CORS

Обновите `Program.cs` для работы с фронтендом:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://your-frontend.netlify.app",
                "https://your-frontend.vercel.app",
                "http://localhost:3000" // для разработки
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// В pipeline перед UseAuthorization:
app.UseCors("AllowFrontend");
```

## Мониторинг и логи

### Railway:
- Dashboard → Logs (в реальном времени)

### Render:
- Dashboard → Logs (в реальном времени)

### ngrok:
- Dashboard → Inspect → http://localhost:4040

## Ограничения Free Tier

### Railway:
- 500 часов/месяц
- 512 MB RAM
- 1 GB storage

### Render:
- Спин-даун после 15 минут неактивности
- 512 MB RAM
- Медленный старт после спин-дауна

### ngrok:
- Случайный URL при каждом запуске (бесплатно)
- 40 соединений/минуту
- Постоянный URL только в платной версии

