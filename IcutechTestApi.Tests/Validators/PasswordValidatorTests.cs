using Xunit;
using FluentAssertions;
using IcutechTestApi.Validators;

namespace IcutechTestApi.Tests.Validators;

public class PasswordValidatorTests
{
    [Theory]
    [InlineData("", false, "Пароль обязателен")]
    [InlineData("12345", false, "Пароль должен содержать минимум 6 символов")]
    [InlineData("abcdef", false, "Пароль должен содержать хотя бы одну цифру")]
    [InlineData("123456", false, "Пароль должен содержать хотя бы одну букву")]
    [InlineData("abc123", true, "")]
    [InlineData("password123", true, "")]
    [InlineData("Test1234", true, "")]
    public void Validate_ShouldReturnExpectedResults(string password, bool expectedValid, string expectedError)
    {
        // Act
        var (isValid, errorMessage) = PasswordValidator.Validate(password);

        // Assert
        isValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            errorMessage.Should().Be(expectedError);
        }
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("abc", false)]
    [InlineData("abc123", false)]
    [InlineData("Abc123", false)]
    [InlineData("Abc123!", false)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("MyStr0ng!Pass", true)]
    public void IsStrongPassword_ShouldReturnExpectedResults(string password, bool expectedStrong)
    {
        // Act
        var result = PasswordValidator.IsStrongPassword(password);

        // Assert
        result.Should().Be(expectedStrong);
    }

    [Theory]
    [InlineData("", "Слабый")]
    [InlineData("abc", "Слабый")]
    [InlineData("abc123", "Слабый")]
    [InlineData("Abc123", "Средний")]
    [InlineData("Abc12345", "Средний")]
    [InlineData("Abc123!", "Средний")]
    [InlineData("Abc123!@#LongPassword", "Очень сильный")]
    public void GetPasswordStrength_ShouldReturnExpectedStrength(string password, string expectedStrength)
    {
        // Act
        var result = PasswordValidator.GetPasswordStrength(password);

        // Assert
        result.Should().Be(expectedStrength);
    }

    [Fact]
    public void Validate_WithVeryLongPassword_ShouldReturnError()
    {
        // Arrange
        var password = new string('a', 101) + "1"; // 102 characters

        // Act
        var (isValid, errorMessage) = PasswordValidator.Validate(password);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("не должен превышать");
    }

    [Fact]
    public void Validate_WithCyrillicCharacters_ShouldBeValid()
    {
        // Arrange
        var password = "Пароль123";

        // Act
        var (isValid, _) = PasswordValidator.Validate(password);

        // Assert
        isValid.Should().BeTrue();
    }
}

