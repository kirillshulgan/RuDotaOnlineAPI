using FluentValidation;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Validators;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}