using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class GrantMemberPortalAccessCommandValidator : AbstractValidator<GrantMemberPortalAccessCommand>
{
    public GrantMemberPortalAccessCommandValidator()
    {
        RuleFor(command => command.MemberId).NotEmpty();
        RuleFor(command => command.GrantedBy).NotEmpty().MaximumLength(450);
    }
}
