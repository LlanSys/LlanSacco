using LS.Application.Features.IAM.Users.Commands;
using LS.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.RoleId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UpdatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new UpdateRoleRequestValidator());
    }
}

