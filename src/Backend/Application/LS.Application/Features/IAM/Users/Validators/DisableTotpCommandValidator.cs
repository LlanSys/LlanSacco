using LS.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class DisableTotpCommandValidator : AbstractValidator<DisableTotpCommand>
{
    public DisableTotpCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.DisabledBy).NotEmpty().MaximumLength(450);
    }
}

