using LS.Application.Features.IAM.Permissions.Commands;
using LS.SharedKernel.Validation.Features.IAM.Permissions.Validators;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

