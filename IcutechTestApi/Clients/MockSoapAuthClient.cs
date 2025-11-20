using IcutechTestApi.Models;
using Microsoft.Extensions.Logging;

namespace IcutechTestApi.Clients;

public class MockSoapAuthClient : ISoapAuthClient
{
    private readonly ILogger<MockSoapAuthClient> _logger;
    private readonly Dictionary<string, (string Password, string Email, string FirstName, string LastName, string Id)> _users;

    public MockSoapAuthClient(ILogger<MockSoapAuthClient> logger)
    {
        _logger = logger;
        _users = new Dictionary<string, (string, string, string, string, string)>
        {
            ["testuser"] = ("password123", "test@example.com", "Test", "User", "test-user-id-001"),
            ["admin"] = ("admin123", "admin@example.com", "Admin", "User", "admin-user-id-002"),
            ["demo"] = ("demo123", "demo@example.com", "Demo", "User", "demo-user-id-003")
        };
    }

    public Task<LoginResult> LoginAsync(string login, string password)
    {
        _logger.LogInformation("MockSoapAuthClient: Attempting login for user: {Login}", login);

        Task.Delay(100).Wait();

        if (_users.TryGetValue(login, out var user))
        {
            if (user.Password == password)
            {
                _logger.LogInformation("MockSoapAuthClient: Login successful for user: {Login}", login);
                return Task.FromResult(new LoginResult
                {
                    Success = true,
                    Message = "Вход выполнен успешно",
                    EntityDetails = new
                    {
                        UserId = user.Id,
                        Login = login,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        LoginTime = DateTime.UtcNow
                    }
                });
            }
            else
            {
                _logger.LogWarning("MockSoapAuthClient: Invalid password for user: {Login}", login);
                return Task.FromResult(new LoginResult
                {
                    Success = false,
                    Message = "Неверный логин или пароль",
                    EntityDetails = null
                });
            }
        }

        _logger.LogWarning("MockSoapAuthClient: User not found: {Login}", login);
        return Task.FromResult(new LoginResult
        {
            Success = false,
            Message = "Неверный логин или пароль",
            EntityDetails = null
        });
    }

    public Task<RegisterResult> RegisterNewCustomerAsync(RegisterRequest request)
    {
        _logger.LogInformation("MockSoapAuthClient: Attempting registration for user: {Login}", request.Login);

        Task.Delay(150).Wait();

        if (_users.ContainsKey(request.Login))
        {
            _logger.LogWarning("MockSoapAuthClient: User already exists: {Login}", request.Login);
            return Task.FromResult(new RegisterResult
            {
                Success = false,
                Message = "Пользователь с таким логином уже существует",
                CreatedCustomerId = null
            });
        }

        var userId = $"user-{Guid.NewGuid():N}";
        _users[request.Login] = (
            request.Password,
            request.Email ?? "",
            request.FirstName ?? "",
            request.LastName ?? "",
            userId
        );

        _logger.LogInformation("MockSoapAuthClient: Registration successful for user: {Login}, ID: {UserId}", 
            request.Login, userId);

        return Task.FromResult(new RegisterResult
        {
            Success = true,
            Message = "Регистрация выполнена успешно",
            CreatedCustomerId = userId
        });
    }

    public IReadOnlyDictionary<string, string> GetRegisteredUsers()
    {
        return _users.ToDictionary(
            kvp => kvp.Key,
            kvp => $"{kvp.Value.FirstName} {kvp.Value.LastName} ({kvp.Value.Email})"
        );
    }
}

