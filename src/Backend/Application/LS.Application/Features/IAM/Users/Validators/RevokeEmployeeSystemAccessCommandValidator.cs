using LS.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class RevokeEmployeeSystemAccessCommandValidator : AbstractValidator<RevokeEmployeeSystemAccessCommand>
{
    public RevokeEmployeeSystemAccessCommandValidator()
    {
        RuleFor(command => command.EmployeeId).NotEmpty();
        RuleFor(command => command.RevokedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Reason).NotEmpty().MaximumLength(500);
    }
}

