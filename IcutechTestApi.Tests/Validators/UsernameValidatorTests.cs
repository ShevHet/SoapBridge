using Xunit;
using FluentAssertions;
using IcutechTestApi.Validators;

namespace IcutechTestApi.Tests.Validators;

public class UsernameValidatorTests
{
    [Theory]
    [InlineData("", false, "Логин обязателен")]
    [InlineData("ab", false, "Логин должен содержать минимум 3 символа")]
    [InlineData("user", true, "")]
    [InlineData("user123", true, "")]
    [InlineData("user_name", true, "")]
    [InlineData("user-name", true, "")]
    [InlineData("User123", true, "")]
    public void Validate_WithLatinCharacters_ShouldReturnExpectedResults(string username, bool expectedValid, string expectedError)
    {
        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            errorMessage.Should().Be(expectedError);
        }
    }

    [Theory]
    [InlineData("пользователь", true, "")]
    [InlineData("Пользователь", true, "")]
    [InlineData("пользователь123", true, "")]
    [InlineData("User_Пользователь", true, "")]
    [InlineData("имя_фамилия", true, "")]
    [InlineData("Иван-Петров", true, "")]
    [InlineData("Алексей2024", true, "")]
    [InlineData("user_русский", true, "")]
    [InlineData("ёж", true, "")]
    [InlineData("Ёжик123", true, "")]
    public void Validate_WithCyrillicCharacters_ShouldBeValid(string username, bool expectedValid, string expectedError)
    {
        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().Be(expectedValid);
        errorMessage.Should().BeEmpty();
    }

    [Theory]
    [InlineData("user@name", false)]
    [InlineData("user name", false)]
    [InlineData("user.name", false)]
    [InlineData("user#123", false)]
    [InlineData("user$name", false)]
    [InlineData("user%name", false)]
    public void Validate_WithInvalidCharacters_ShouldReturnError(string username, bool expectedValid)
    {
        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("только латинские и русские буквы");
    }

    [Theory]
    [InlineData("-username", false)]
    [InlineData("_username", false)]
    [InlineData("username-", false)]
    [InlineData("username_", false)]
    [InlineData("-пользователь", false)]
    [InlineData("пользователь-", false)]
    public void Validate_WithInvalidStartOrEnd_ShouldReturnError(string username, bool expectedValid)
    {
        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("не должен начинаться или заканчиваться");
    }

    [Fact]
    public void Validate_WithTooLongUsername_ShouldReturnError()
    {
        // Arrange
        var username = new string('a', 51); // 51 characters

        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("не должен превышать 50 символов");
    }

    [Fact]
    public void Validate_WithMaxLengthUsername_ShouldBeValid()
    {
        // Arrange
        var username = new string('a', 50); // Exactly 50 characters

        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }

    [Theory]
    [InlineData("пользователь", true)]
    [InlineData("User123", true)]
    [InlineData("user-name", true)]
    [InlineData("-username", false)]
    [InlineData("", false)]
    public void IsValid_ShouldReturnExpectedResults(string username, bool expected)
    {
        // Act
        var result = UsernameValidator.IsValid(username);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Validate_WithMixedCyrillicAndLatinCharacters_ShouldBeValid()
    {
        // Arrange
        var username = "UserИмя123";

        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithWhitespaceOnly_ShouldReturnError()
    {
        // Arrange
        var username = "   ";

        // Act
        var (isValid, errorMessage) = UsernameValidator.Validate(username);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Логин обязателен");
    }
}

