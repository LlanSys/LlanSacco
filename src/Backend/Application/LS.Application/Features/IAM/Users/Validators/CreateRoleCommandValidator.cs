using LS.Application.Features.IAM.Users.Commands;
using LS.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.CreatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new CreateRoleRequestValidator());
    }
}

