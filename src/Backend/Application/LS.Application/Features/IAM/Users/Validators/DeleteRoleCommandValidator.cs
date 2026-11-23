using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(request => request.RoleId).NotEmpty();
    }
}
