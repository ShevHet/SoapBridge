using System.Text.RegularExpressions;

namespace IcutechTestApi.Validators;

public static class UsernameValidator
{
    private const int MinLength = 3;
    private const int MaxLength = 50;

    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_-]+$",
        RegexOptions.Compiled
    );

    public static (bool IsValid, string ErrorMessage) Validate(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return (false, "Логин обязателен");
        }

        if (username.Length < MinLength)
        {
            return (false, $"Логин должен содержать минимум {MinLength} символа");
        }

        if (username.Length > MaxLength)
        {
            return (false, $"Логин не должен превышать {MaxLength} символов");
        }

        if (!UsernameRegex.IsMatch(username))
        {
            return (false, "Логин может содержать только латинские буквы, цифры, дефис и подчеркивание");
        }

        if (username.StartsWith('-') || username.StartsWith('_') || 
            username.EndsWith('-') || username.EndsWith('_'))
        {
            return (false, "Логин не должен начинаться или заканчиваться на дефис или подчеркивание");
        }

        return (true, string.Empty);
    }

    public static bool IsValid(string username)
    {
        var (isValid, _) = Validate(username);
        return isValid;
    }
}

