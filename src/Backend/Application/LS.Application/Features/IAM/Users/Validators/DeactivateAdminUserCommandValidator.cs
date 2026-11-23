using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class DeactivateAdminUserCommandValidator : AbstractValidator<DeactivateAdminUserCommand>
{
    public DeactivateAdminUserCommandValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Request).NotNull();
    }
}
