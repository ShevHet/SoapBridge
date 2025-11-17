using FluentValidation;
using IcutechTestApi.DTOs;

namespace IcutechTestApi.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен для заполнения")
            .MaximumLength(100).WithMessage("Логин не может быть длиннее 100 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения")
            .MinimumLength(3).WithMessage("Пароль должен содержать минимум 3 символа")
            .MaximumLength(100).WithMessage("Пароль не может быть длиннее 100 символов");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный формат email")
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(255).WithMessage("Email не может быть длиннее 255 символов");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Имя не может быть длиннее 100 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Фамилия не может быть длиннее 100 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}

