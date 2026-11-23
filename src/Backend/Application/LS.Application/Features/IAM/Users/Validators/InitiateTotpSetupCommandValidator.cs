using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class InitiateTotpSetupCommandValidator : AbstractValidator<InitiateTotpSetupCommand>
{
    public InitiateTotpSetupCommandValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
    }
}
