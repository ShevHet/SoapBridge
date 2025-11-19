# Icutech Test API

Небольшой pet-project в формате “тестовое задание”: backend на .NET 9, который ходит в SOAP‑сервис, и простая одностраничка на чистом JS/Bootstrap. Код собран в стиле “микро clean architecture”: контроллеры, сервисы, клиенты, DTO, валидаторы.

Главная цель — быстро поднять окружение, запустить API, прогнать тесты и показать фронту рабочие REST эндпоинты (пусть даже внешний SOAP сейчас не отвечает как надо).

## Что внутри

- `IcutechTestApi` — Web API + SOAP клиент + минимальный фронтенд (в `wwwroot`)
- `IcutechTestApi.Tests` — unit тесты (xUnit + Moq)
- `tests/e2e` — Playwright сценарии для UI
- `scripts/checklist.js` — автоматический чеклист (npm run check)

## Требования

- .NET 9 SDK
- Node.js 18+ (правило: одна установка для фронта и Playwright)
- Docker (если нужен запуск в контейнере)

## Как запустить локально

```bash
git clone https://github.com/your-username/icutech-test-api.git
cd icutech-test-api

# Backend
cd IcutechTestApi
dotnet restore
dotnet run
# --> http://localhost:5030, swagger на https://localhost:7093/swagger

# Frontend (production версия)
cd wwwroot
npm install
npm run build:prod
# Откройте index-prod.html в браузере
```

### Быстрые команды

```bash
npm run check        # прогон локального чеклиста (API, фронт, lighthouse)
npm run test:unit    # dotnet test
npm run test:e2e     # Playwright (нужен запущенный API)
npm run build:frontend
```

### Docker

```bash
cd IcutechTestApi
docker-compose up --build
# API слушает http://localhost:8080
```

## Структура

```
.
├── IcutechTestApi
│   ├── Clients / Controllers / Services / Validators
│   ├── wwwroot (index.html, app.js, styles.css, build scripts)
│   ├── Dockerfile, docker-compose.yml
│   └── Program.cs (Swagger, CORS, сжатие, статика)
├── IcutechTestApi.Tests (AuthControllerTests)
├── tests/e2e (Playwright + конфиги)
├── scripts/checklist.js
├── README.md, TESTING_GUIDE.md
└── package.json (общие npm-скрипты)
```

## Тесты

- `dotnet test` — unit тесты контроллера (Moq + FluentAssertions)
- `npm run test:e2e` — сценарии Playwright (валидные/невалидные формы, ретраи, табы)
- `npm run test:pagespeed` — lighthouse CLI (100 mobile / ≥90 desktop)

## Известные ограничения

- SOAP‑эндпоинт `http://isapi.mekashron.com/icu-tech/icutech-test.dll` последние месяцы отдаёт HTML (страницу с кнопками WSDL) вместо SOAP XML.
- В результате `/api/auth/login` и `/api/auth/register` получают 200 от внешнего сервиса, но не могут распарсить ответ → backend возвращает 500 с сообщением “Внутренняя ошибка сервера при обращении к сервису …”.
- Это воспроизводится у всех: проблема на стороне внешнего сервиса. В коде всё завёрнуто в `SoapClientException`, логи содержат полный текст ответа. Для демо можно замокать SOAP клиент или использовать заранее подготовленные ответы.
- Подробности и сценарии диагностики — в [TESTING_GUIDE.md](TESTING_GUIDE.md#проблема-ошибка-500-internal-server-error-при-регистрациивходе).

## Разработка

- DTO + валидаторы лежат рядом с контроллерами — проще ориентироваться
- Конфиг SOAP сервиса в `appsettings.json` (`SoapService:Url`), можно переопределить переменными окружения
- Критический CSS инлайнится в `index-prod.html`, остальное грузится через `preload`
- В `app.js` есть `fetchWithRetry`: при сетевой ошибке делает одну повторную попытку с экспоненциальной задержкой

## Документация

- [TESTING_GUIDE.md](TESTING_GUIDE.md) — всё про тесты, чеклисты и диагностику
- [DEPLOY_BACKEND.md](DEPLOY_BACKEND.md) / [DEPLOY_FRONTEND.md](DEPLOY_FRONTEND.md) — шпаргалки по Railway/Render/Netlify
- `IcutechTestApi/wwwroot/BUILD_INSTRUCTIONS.md` — если нужно собрать фронтенд отдельно

## Если хочется подчистить артефакты

Стандартный набор:

```bash
git clean -fdX                 # убирает bin/obj/node_modules, оставляет tracked файлы
dotnet clean IcutechTestApi    # очистить сборку
```

`.gitignore` уже настроен под bin/obj/node_modules, поэтому в репозиторий они не попадут.

---

Если что-то не сходится с инструкциями — смотри лог `dotnet run` (там пишутся все ошибки SOAP) или пингуй меня через issues. README старался оставить максимально “живым”, без роботизированных формулировок. Удачи!

