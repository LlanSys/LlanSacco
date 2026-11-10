using LS.Application.Features.IAM.Users.Commands;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class GrantEmployeeSystemAccessCommandValidator : AbstractValidator<GrantEmployeeSystemAccessCommand>
{
    public GrantEmployeeSystemAccessCommandValidator()
    {
        RuleFor(command => command.EmployeeId).NotEmpty();
        RuleFor(command => command.GrantedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Roles).NotNull().Must(static roles => roles.Count > 0)
            .WithMessage("At least one role is required.");
        RuleForEach(command => command.Roles).NotEmpty().MaximumLength(80);
    }
}

