# IcutechTestApi

Web API проект на .NET 9 с архитектурой Clean-ish.

## Структура проекта

```
IcutechTestApi/
├── Controllers/      # API контроллеры
├── Services/         # Бизнес-логика и сервисы
├── DTOs/            # Data Transfer Objects
├── Clients/         # Клиенты для внешних API
├── Models/          # Модели данных
├── Properties/      # Настройки проекта
├── Program.cs       # Точка входа приложения
└── Dockerfile       # Docker образ
```

## Требования

- .NET 9 SDK
- Docker и Docker Compose (опционально)

## Запуск проекта

### Локальный запуск

```bash
dotnet restore
dotnet run
```

API будет доступен по адресу:
- HTTP: `http://localhost:5030`
- HTTPS: `https://localhost:7093`
- Swagger UI: `https://localhost:7093/swagger`

### Запуск через Docker

```bash
docker-compose up --build
```

API будет доступен по адресу:
- HTTP: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

## API Endpoints

### GET /api/example
Получить пример данных.

### POST /api/example
Создать новый пример данных.

**Тело запроса:**
```json
{
  "name": "Example Name"
}
```

### POST /api/auth/login
Выполнить вход пользователя через SOAP-сервис.

**Тело запроса:**
```json
{
  "login": "username",
  "password": "password"
}
```

**Ответ:**
```json
{
  "success": true,
  "message": "Login successful",
  "entityDetails": "..."
}
```

### POST /api/auth/register
Зарегистрировать нового клиента через SOAP-сервис.

**Тело запроса:**
```json
{
  "login": "newuser",
  "password": "securepassword",
  "email": "user@example.com",
  "firstName": "Иван",
  "lastName": "Иванов"
}
```

**Ответ:**
```json
{
  "success": true,
  "message": "Registration successful",
  "createdCustomerId": "12345"
}
```

## SOAP-клиент

Проект включает SOAP-клиент для работы с сервисом `http://isapi.mekashron.com/icu-tech/icutech-test.dll`.

### Конфигурация

Настройки SOAP-сервиса находятся в `appsettings.json`:

```json
{
  "SoapService": {
    "Url": "http://isapi.mekashron.com/icu-tech/icutech-test.dll"
  }
}
```

### Использование

SOAP-клиент зарегистрирован в DI-контейнере и может быть использован в контроллерах и сервисах:

```csharp
public class MyService
{
    private readonly ISoapAuthClient _soapAuthClient;

    public MyService(ISoapAuthClient soapAuthClient)
    {
        _soapAuthClient = soapAuthClient;
    }

    public async Task<LoginResult> LoginAsync(string login, string password)
    {
        try
        {
            return await _soapAuthClient.LoginAsync(login, password);
        }
        catch (SoapClientException ex)
        {
            // Обработка ошибок SOAP-клиента
            Console.WriteLine($"Ошибка: {ex.Message}");
            throw;
        }
    }
}
```

### Обработка ошибок

SOAP-клиент обрабатывает следующие типы ошибок:
- **HttpRequestException** - ошибки сети
- **TaskCanceledException** - тайм-ауты
- **SoapClientException** - специфичные ошибки SOAP-клиента

Все ошибки логируются и оборачиваются в понятные исключения с информативными сообщениями.

## Технологии

- .NET 9
- ASP.NET Core Web API
- Swagger/OpenAPI
- Docker
- SOAP (HttpClient-based)

