using Microsoft.AspNetCore.Mvc;
using IcutechTestApi.Clients;
using IcutechTestApi.Models;
using IcutechTestApi.DTOs;

namespace IcutechTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISoapAuthClient _soapAuthClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISoapAuthClient soapAuthClient, ILogger<AuthController> logger)
    {
        _soapAuthClient = soapAuthClient;
        _logger = logger;
    }

    /// <summary>
    /// Выполняет вход пользователя через SOAP-сервис
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <returns>Результат входа</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("Попытка входа для пользователя: {Login}", request.Login);

            var result = await _soapAuthClient.LoginAsync(request.Login, request.Password);

            if (result.Success)
            {
                _logger.LogInformation("Успешный вход для пользователя: {Login}", request.Login);
                return Ok(new AuthResponse<object>
                {
                    Success = true,
                    Entity = result.EntityDetails
                });
            }
            else
            {
                _logger.LogWarning("Ошибка аутентификации для пользователя: {Login}. Сообщение: {Message}", 
                    request.Login, result.Message);
                return Unauthorized(new AuthErrorResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры запроса для входа");
            return BadRequest(new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (SoapClientException ex)
        {
            _logger.LogError(ex, "Ошибка SOAP-клиента при входе");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера при обращении к сервису аутентификации"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при входе");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера"
            });
        }
    }

    /// <summary>
    /// Регистрирует нового клиента через SOAP-сервис
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <returns>Результат регистрации</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            _logger.LogInformation("Попытка регистрации для пользователя: {Login}", request.Login);

            var registerRequest = new RegisterRequest
            {
                Login = request.Login,
                Password = request.Password,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _soapAuthClient.RegisterNewCustomerAsync(registerRequest);

            if (result.Success)
            {
                _logger.LogInformation("Успешная регистрация для пользователя: {Login}. ID: {CustomerId}", 
                    request.Login, result.CreatedCustomerId);
                return Ok(new RegisterResponse
                {
                    Success = true,
                    Message = result.Message,
                    CreatedCustomerId = result.CreatedCustomerId
                });
            }
            else
            {
                _logger.LogWarning("Ошибка регистрации для пользователя: {Login}. Сообщение: {Message}", 
                    request.Login, result.Message);
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры запроса для регистрации");
            return BadRequest(new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (SoapClientException ex)
        {
            _logger.LogError(ex, "Ошибка SOAP-клиента при регистрации");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера при обращении к сервису регистрации"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при регистрации");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера"
            });
        }
    }
}

