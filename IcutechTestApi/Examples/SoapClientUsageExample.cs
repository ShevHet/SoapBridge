using IcutechTestApi.Clients;
using IcutechTestApi.Models;

namespace IcutechTestApi.Examples;

/// <summary>
/// Пример использования SOAP-клиента
/// </summary>
public class SoapClientUsageExample
{
    private readonly ISoapAuthClient _soapAuthClient;

    public SoapClientUsageExample(ISoapAuthClient soapAuthClient)
    {
        _soapAuthClient = soapAuthClient;
    }

    /// <summary>
    /// Пример вызова метода LoginAsync
    /// </summary>
    public async Task<LoginResult> ExampleLoginAsync()
    {
        try
        {
            // Вызов метода входа
            var loginResult = await _soapAuthClient.LoginAsync("testuser", "testpassword");

            if (loginResult.Success)
            {
                Console.WriteLine($"Вход выполнен успешно: {loginResult.Message}");
                Console.WriteLine($"Детали: {loginResult.EntityDetails}");
            }
            else
            {
                Console.WriteLine($"Ошибка входа: {loginResult.Message}");
            }

            return loginResult;
        }
        catch (SoapClientException ex)
        {
            // Обработка ошибок SOAP-клиента
            Console.WriteLine($"Ошибка SOAP-клиента: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            throw;
        }
        catch (ArgumentException ex)
        {
            // Обработка ошибок валидации параметров
            Console.WriteLine($"Некорректные параметры: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Пример вызова метода RegisterNewCustomerAsync
    /// </summary>
    public async Task<RegisterResult> ExampleRegisterAsync()
    {
        try
        {
            // Создание запроса на регистрацию
            var registerRequest = new RegisterRequest
            {
                Login = "newuser",
                Password = "securepassword123",
                Email = "newuser@example.com",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            // Вызов метода регистрации
            var registerResult = await _soapAuthClient.RegisterNewCustomerAsync(registerRequest);

            if (registerResult.Success)
            {
                Console.WriteLine($"Регистрация выполнена успешно: {registerResult.Message}");
                if (!string.IsNullOrEmpty(registerResult.CreatedCustomerId))
                {
                    Console.WriteLine($"ID созданного клиента: {registerResult.CreatedCustomerId}");
                }
            }
            else
            {
                Console.WriteLine($"Ошибка регистрации: {registerResult.Message}");
            }

            return registerResult;
        }
        catch (SoapClientException ex)
        {
            // Обработка ошибок SOAP-клиента
            Console.WriteLine($"Ошибка SOAP-клиента: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }
            throw;
        }
        catch (ArgumentNullException ex)
        {
            // Обработка ошибок валидации параметров
            Console.WriteLine($"Отсутствует обязательный параметр: {ex.Message}");
            throw;
        }
        catch (ArgumentException ex)
        {
            // Обработка ошибок валидации параметров
            Console.WriteLine($"Некорректные параметры: {ex.Message}");
            throw;
        }
    }
}

