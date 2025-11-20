using System.Text.RegularExpressions;

namespace IcutechTestApi.Validators;

public static class PasswordValidator
{
    private const int MinLength = 6;
    private const int MaxLength = 100;

    public static (bool IsValid, string ErrorMessage) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Пароль обязателен");
        }

        if (password.Length < MinLength)
        {
            return (false, $"Пароль должен содержать минимум {MinLength} символов");
        }

        if (password.Length > MaxLength)
        {
            return (false, $"Пароль не должен превышать {MaxLength} символов");
        }

        var hasLetter = Regex.IsMatch(password, @"[a-zA-Zа-яА-Я]");
        var hasDigit = Regex.IsMatch(password, @"\d");

        if (!hasLetter)
        {
            return (false, "Пароль должен содержать хотя бы одну букву");
        }

        if (!hasDigit)
        {
            return (false, "Пароль должен содержать хотя бы одну цифру");
        }

        return (true, string.Empty);
    }

    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"\d");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    public static string GetPasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Слабый";

        int score = 0;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (Regex.IsMatch(password, @"[a-z]")) score++;
        if (Regex.IsMatch(password, @"[A-Z]")) score++;
        if (Regex.IsMatch(password, @"\d")) score++;
        if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) score++;

        return score switch
        {
            <= 2 => "Слабый",
            3 or 4 => "Средний",
            5 => "Сильный",
            _ => "Очень сильный"
        };
    }
}

