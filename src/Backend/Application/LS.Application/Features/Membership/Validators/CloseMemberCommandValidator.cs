using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class CloseMemberCommandValidator : AbstractValidator<CloseMemberCommand>
{
    public CloseMemberCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty();
    }
}
