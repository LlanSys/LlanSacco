using LS.Application.Features.IAM.Permissions.Commands;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

