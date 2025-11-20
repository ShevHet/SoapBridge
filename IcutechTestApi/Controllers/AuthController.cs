using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IcutechTestApi.Clients;
using IcutechTestApi.DTOs;
using IcutechTestApi.Models;
using IcutechTestApi.Validators;

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Login and password are required"
                });
            }

            var (isUsernameValid, usernameError) = UsernameValidator.Validate(request.Login);
            if (!isUsernameValid)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = usernameError
                });
            }

            var result = await _soapAuthClient.LoginAsync(request.Login, request.Password);

            if (result.Success)
            {
                return Ok(new AuthResponse<object>
                {
                    Success = true,
                    Message = result.Message,
                    Entity = result.EntityDetails
                });
            }

            return Unauthorized(new AuthErrorResponse
            {
                Success = false,
                Message = result.Message
            });
        }
        catch (SoapClientException ex)
        {
            _logger.LogError(ex, "SOAP client error during login");
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера"
            });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Login and password are required"
                });
            }

            var (isUsernameValid, usernameError) = UsernameValidator.Validate(request.Login);
            if (!isUsernameValid)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = usernameError
                });
            }

            var (isPasswordValid, passwordError) = PasswordValidator.Validate(request.Password);
            if (!isPasswordValid)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = passwordError
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var (isEmailValid, emailError) = EmailValidator.Validate(request.Email);
                if (!isEmailValid)
                {
                    return BadRequest(new AuthErrorResponse
                    {
                        Success = false,
                        Message = emailError
                    });
                }
            }

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
                return Ok(new RegisterResponse
                {
                    Success = true,
                    Message = result.Message,
                    CreatedCustomerId = result.CreatedCustomerId
                });
            }

            return BadRequest(new AuthErrorResponse
            {
                Success = false,
                Message = result.Message
            });
        }
        catch (SoapClientException ex)
        {
            _logger.LogError(ex, "SOAP client error during registration");
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new AuthErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = "Внутренняя ошибка сервера"
            });
        }
    }
}

