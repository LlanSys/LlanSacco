using LS.Application.Features.IAM.Users.Commands;
using LS.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new RefreshTokenRequestValidator());
    }
}

