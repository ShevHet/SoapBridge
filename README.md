# Icutech Test API

[![CI/CD Pipeline](https://github.com/your-username/icutech-test-api/actions/workflows/ci.yml/badge.svg)](https://github.com/your-username/icutech-test-api/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Полнофункциональный тестовый проект: backend на .NET 9, который взаимодействует с SOAP‑сервисом, и современная одностраничка на чистом JS/Bootstrap. Код организован в стиле "микро clean architecture": контроллеры, сервисы, клиенты, DTO, валидаторы.

**Главная цель** — продемонстрировать полный цикл разработки: от локальной разработки до production деплоя с автоматическими тестами, оптимизацией производительности (Lighthouse 100/100) и CI/CD.

## 🎯 Что внутри

- **Backend (`IcutechTestApi`)** — Web API на .NET 9 + SOAP клиент + статический фронтенд (в `wwwroot`)
- **Frontend (`wwwroot`)** — Single Page Application на Bootstrap 5 + Vanilla JS с production сборкой
- **Tests (`IcutechTestApi.Tests`)** — Unit тесты (xUnit + Moq + FluentAssertions)
- **E2E Tests (`tests/e2e`)** — Playwright сценарии для UI тестирования
- **CI/CD (`.github/workflows`)** — Автоматические тесты, сборка, security scan
- **Scripts (`scripts/checklist.js`)** — Автоматический чеклист с Lighthouse проверками
- **Документация** — Подробные гайды по тестированию, деплою и сборке

## 📋 Требования

- .NET 9 SDK ([скачать](https://dotnet.microsoft.com/download/dotnet/9.0))
- Node.js 18+ ([скачать](https://nodejs.org/))
- Docker (опционально, для контейнерного запуска)
- Git

## 🚀 Быстрый старт

### Клонирование и запуск

```bash
git clone https://github.com/your-username/icutech-test-api.git
cd icutech-test-api

# 1. Запустите Backend API
cd IcutechTestApi
dotnet restore
dotnet run
# --> API: http://localhost:5030
# --> Swagger: http://localhost:5030/swagger
# --> Frontend: http://localhost:5030

# 2. (Опционально) Соберите production версию фронтенда
cd wwwroot
npm install
npm run build:prod
# Создаются: index-prod.html, styles.min.css, app.min.js
```

### 🐳 Запуск через Docker

```bash
# Вариант 1: docker-compose (рекомендуется)
docker-compose up --build
# API слушает http://localhost:8080

# Вариант 2: отдельный контейнер
docker build -t icutech-test-api .
docker run -p 8080:10000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e USE_MOCK_SOAP_CLIENT=true \
  icutech-test-api
```

### 🛠️ Полезные команды

```bash
# Тестирование
npm run test:unit          # Unit тесты backend (dotnet test)
npm run test:e2e           # E2E тесты Playwright (требует запущенный API)
npm run check              # Полный чеклист (API, фронт, lighthouse)
npm run check:prod         # Чеклист для production URLs

# Сборка
npm run build:frontend     # Собрать production фронтенд
dotnet build               # Собрать backend

# Очистка
dotnet clean               # Очистить сборку backend
cd IcutechTestApi/wwwroot && npm run clean  # Удалить собранные файлы
```

## 📁 Структура проекта

```
.
├── .github/workflows/
│   └── ci.yml                      # GitHub Actions CI/CD
├── IcutechTestApi/                 # Backend API
│   ├── Clients/                    # SOAP клиенты
│   │   ├── ISoapAuthClient.cs
│   │   ├── SoapAuthClient.cs
│   │   └── MockSoapAuthClient.cs
│   ├── Controllers/                # REST API контроллеры
│   │   ├── AuthController.cs
│   │   ├── UserProfileController.cs
│   │   └── HealthController.cs
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Models/                     # Доменные модели
│   ├── Validators/                 # Валидаторы
│   ├── Middleware/                 # Middleware (логирование, rate limiting)
│   ├── wwwroot/                    # Статический фронтенд
│   │   ├── index.html              # Dev версия
│   │   ├── index-prod.html         # Production (генерируется)
│   │   ├── app.js / app.min.js
│   │   ├── styles.css / styles.min.css
│   │   ├── package.json            # Build скрипты
│   │   └── build-prod.js           # Скрипт сборки production HTML
│   ├── Program.cs                  # Точка входа (CORS, Swagger, DI)
│   ├── appsettings.json            # Конфигурация
│   └── Dockerfile
├── IcutechTestApi.Tests/           # Unit тесты
│   ├── Controllers/
│   └── Validators/
├── tests/e2e/                      # E2E тесты (Playwright)
│   ├── specs/auth.spec.js
│   └── playwright.config.js
├── scripts/
│   └── checklist.js                # Автоматический чеклист
├── Dockerfile                      # Docker образ для всего проекта
├── docker-compose.yml              # Docker Compose конфигурация
├── netlify.toml                    # Конфигурация для Netlify
├── render.yaml                     # Конфигурация для Render
├── package.json                    # Root npm скрипты
└── README.md                       # Этот файл
```

## 🧪 Тестирование

### Unit тесты (xUnit + Moq)

```bash
dotnet test                           # Все unit тесты
dotnet test --logger "console;verbosity=detailed"
dotnet test --collect:"XPlat Code Coverage"  # С coverage
```

**Что покрывается:**
- ✅ AuthController (login, register, валидация)
- ✅ HealthController (health checks)
- ✅ Валидаторы (email, password, username)

### E2E тесты (Playwright)

```bash
cd tests/e2e
npm install
npm test                              # Все браузеры
npm test -- --project=chromium        # Только Chrome
npm test -- --headed                  # С UI
npm test -- --debug                   # Debug режим
```

**Сценарии:**
- ✅ Логин (валидный/невалидный, loading state, retry logic)
- ✅ Регистрация (валидация, success/error)
- ✅ Переключение табов
- ✅ Обработка сетевых ошибок

### Автоматический чеклист

```bash
npm run check                         # Локальная проверка
npm run check:prod                    # Production URLs
```

**Проверяет:**
- Backend endpoints (API, Swagger)
- Frontend файлы (index-prod.html, minified CSS/JS)
- Lighthouse scores (Mobile 100, Desktop ≥90)
- Production URLs (если заданы)

## 🚢 Деплой

### Frontend (Netlify/Vercel/GitHub Pages)

```bash
# Подготовка
cd IcutechTestApi/wwwroot
npm install && npm run build:prod

# Netlify CLI
netlify deploy --prod

# Или через UI: перетащите wwwroot/ на netlify.com
```

**Конфигурация:** Готовый `netlify.toml` в корне проекта.

### Backend (Railway/Render/Azure)

**Railway (рекомендуется):**
1. Подключите GitHub репозиторий
2. Railway автоматически найдет `Dockerfile`
3. Deploy автоматически при push в `main`

**Render:**
1. Используйте готовый `render.yaml`
2. Подключите через Blueprint
3. Free tier (спит после 15 мин)

**ngrok (для демо):**
```bash
dotnet run
ngrok http 5030
```

## ⚡ Performance

Frontend оптимизирован для максимальной производительности:

- ✅ **Lighthouse Mobile**: 100/100
- ✅ **Lighthouse Desktop**: ≥ 90/100
- ✅ Inline критический CSS
- ✅ Минификация CSS/JS (clean-css + terser)
- ✅ Defer для скриптов
- ✅ Preload для ресурсов
- ✅ Gzip/Brotli на хостинге

## ⚠️ Известные ограничения

- **SOAP сервис:** Использует RPC/encoded стиль SOAP с namespace `urn:ICUTech.Intf-IICUTech`
- **Решение для тестирования:** Используйте Mock клиент (`USE_MOCK_SOAP_CLIENT=true`) в `appsettings.json`
- **В production:** Настройте правильный URL SOAP endpoint в конфигурации

## 🔄 CI/CD

Проект использует GitHub Actions для автоматизации:

### Workflow: CI Pipeline (`.github/workflows/ci.yml`)

**Триггеры:** Push в `main`/`develop`, Pull Requests

**Jobs:**
- ✅ **Backend Tests** — Unit тесты (xUnit), coverage report
- ✅ **Frontend Build** — Production сборка, проверка файлов
- ✅ **E2E Tests** — Playwright тесты (все браузеры)
- ✅ **Docker Build** — Сборка и тест Docker образа
- ✅ **Checklist** — Автоматический чеклист
- ✅ **Security Scan** — Trivy vulnerability scanner

**Просмотр:** GitHub → Actions → выберите workflow run

## 💻 Разработка

### Архитектура

- **Clean Architecture**: Контроллеры → Services → Clients
- **DTO pattern**: Валидация и маппинг на уровне контроллеров
- **Dependency Injection**: Все зависимости через DI контейнер
- **Middleware**: Логирование, rate limiting, error handling

### Конфигурация

**appsettings.json:**
```json
{
  "SoapService": {
    "Url": "http://isapi.mekashron.com/icu-tech/icutech-test.dll",
    "UseMockClient": false
  }
}
```

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT` — Production/Development
- `USE_MOCK_SOAP_CLIENT` — true/false
- `PORT` — Порт для API

### Frontend

- **Vanilla JS** — Без фреймворков
- **Bootstrap 5** — Через CDN
- **Retry Logic**: `fetchWithRetry` с экспоненциальной задержкой
- **Build Process**: npm scripts → clean-css + terser → production HTML


## 🛠️ Утилиты

### Очистка артефактов

```bash
# Удалить bin/obj/node_modules
git clean -fdX

# Очистить .NET сборку
dotnet clean

# Удалить production файлы фронтенда
cd IcutechTestApi/wwwroot && npm run clean
```

### Генерация coverage report

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## 🤝 Contributing

1. Fork репозитория
2. Создайте feature branch: `git checkout -b feature/amazing-feature`
3. Commit изменения: `git commit -m 'Add amazing feature'`
4. Push в branch: `git push origin feature/amazing-feature`
5. Создайте Pull Request

**Требования:**
- ✅ Все тесты проходят
- ✅ Code coverage не снижается
- ✅ Следуйте существующему code style
- ✅ Обновите документацию (если нужно)

## 📝 License

Этот проект использует MIT License. См. [LICENSE](LICENSE) для деталей.

## 🙏 Благодарности

- .NET Community
- Bootstrap Team
- Playwright Team
- Railway & Render за бесплатный хостинг
---

**Made with ❤️ and .NET 9**

*README старался оставить максимально "живым", без роботизированных формулировок. Удачи!*

