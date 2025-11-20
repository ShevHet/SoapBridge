using Microsoft.AspNetCore.Mvc;
using IcutechTestApi.DTOs;
using IcutechTestApi.Validators;

namespace IcutechTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(ILogger<UserProfileController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public IActionResult GetProfile(string userId)
    {
        _logger.LogInformation("Fetching profile for user: {UserId}", userId);

        var profile = new UserProfileDto
        {
            UserId = userId,
            Login = "demo_user",
            Email = "demo@example.com",
            FirstName = "Демо",
            LastName = "Пользователь",
            FullName = "Демо Пользователь",
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            LastLoginAt = DateTime.UtcNow.AddHours(-2)
        };

        return Ok(new ProfileResponse
        {
            Success = true,
            Message = "Профиль успешно получен",
            Profile = profile
        });
    }

    [HttpPut("{userId}")]
    public IActionResult UpdateProfile(string userId, [FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            _logger.LogInformation("Updating profile for user: {UserId}", userId);

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

            var updatedProfile = new UserProfileDto
            {
                UserId = userId,
                Login = "demo_user",
                Email = request.Email ?? "demo@example.com",
                FirstName = request.FirstName ?? "Демо",
                LastName = request.LastName ?? "Пользователь",
                FullName = $"{request.FirstName ?? "Демо"} {request.LastName ?? "Пользователь"}",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                LastLoginAt = DateTime.UtcNow
            };

            return Ok(new ProfileResponse
            {
                Success = true,
                Message = "Профиль успешно обновлен",
                Profile = updatedProfile
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = "Ошибка при обновлении профиля"
            });
        }
    }

    [HttpPost("{userId}/change-password")]
    public IActionResult ChangePassword(string userId, [FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            _logger.LogInformation("Attempting to change password for user: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Текущий пароль обязателен"
                });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Новый пароль обязателен"
                });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Новый пароль и подтверждение не совпадают"
                });
            }

            if (request.NewPassword == request.CurrentPassword)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = "Новый пароль должен отличаться от текущего"
                });
            }

            var (isPasswordValid, passwordError) = PasswordValidator.Validate(request.NewPassword);
            if (!isPasswordValid)
            {
                return BadRequest(new AuthErrorResponse
                {
                    Success = false,
                    Message = passwordError
                });
            }

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

            return Ok(new AuthErrorResponse
            {
                Success = true,
                Message = "Пароль успешно изменен"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = "Ошибка при изменении пароля"
            });
        }
    }

    [HttpDelete("{userId}")]
    public IActionResult DeleteAccount(string userId)
    {
        try
        {
            _logger.LogWarning("Attempting to delete account for user: {UserId}", userId);

            return Ok(new AuthErrorResponse
            {
                Success = true,
                Message = "Аккаунт успешно удален"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account for user: {UserId}", userId);
            return StatusCode(500, new AuthErrorResponse
            {
                Success = false,
                Message = "Ошибка при удалении аккаунта"
            });
        }
    }

    [HttpPost("check-password-strength")]
    public IActionResult CheckPasswordStrength([FromBody] string password)
    {
        var strength = PasswordValidator.GetPasswordStrength(password);
        var (isValid, error) = PasswordValidator.Validate(password);
        var isStrong = PasswordValidator.IsStrongPassword(password);

        return Ok(new
        {
            strength,
            isValid,
            isStrong,
            message = isValid ? strength : error
        });
    }
}

