using FluentValidation;
using LS.Application.Features.IAM.Permissions.Commands;
using LS.SharedKernel.Validation.Features.IAM.Permissions.Validators;

namespace LS.Application.Features.IAM.Permissions.Validators;

internal sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
