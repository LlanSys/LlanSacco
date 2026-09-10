using LS.Application.Features.IAM.Users.Commands;
using LS.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.LoginRequest)
            .NotNull()
            .SetValidator(new LoginRequestValidator());
    }
}

