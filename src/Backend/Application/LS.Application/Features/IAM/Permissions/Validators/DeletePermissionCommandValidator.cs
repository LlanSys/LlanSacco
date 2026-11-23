using FluentValidation;
using LS.Application.Features.IAM.Permissions.Commands;

namespace LS.Application.Features.IAM.Permissions.Validators;

internal sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
