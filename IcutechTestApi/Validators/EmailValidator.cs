using System.Text.RegularExpressions;

namespace IcutechTestApi.Validators;

public static class EmailValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex StrictEmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static (bool IsValid, string ErrorMessage) Validate(string email, bool strict = false)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email обязателен");
        }

        if (email.Length > 254) // RFC 5321
        {
            return (false, "Email слишком длинный");
        }

        var regex = strict ? StrictEmailRegex : EmailRegex;
        
        if (!regex.IsMatch(email))
        {
            return (false, "Неверный формат email");
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return (false, "Email должен содержать один символ @");
        }

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length > 64) // RFC 5321
        {
            return (false, "Локальная часть email слишком длинная");
        }

        if (domain.Length < 3 || !domain.Contains('.'))
        {
            return (false, "Неверный домен email");
        }

        return (true, string.Empty);
    }

    public static bool IsValid(string email)
    {
        var (isValid, _) = Validate(email);
        return isValid;
    }
}

