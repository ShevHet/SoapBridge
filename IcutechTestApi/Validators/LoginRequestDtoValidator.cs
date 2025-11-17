using FluentValidation;
using IcutechTestApi.DTOs;

namespace IcutechTestApi.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен для заполнения")
            .MaximumLength(100).WithMessage("Логин не может быть длиннее 100 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения")
            .MinimumLength(3).WithMessage("Пароль должен содержать минимум 3 символа")
            .MaximumLength(100).WithMessage("Пароль не может быть длиннее 100 символов");
    }
}

