using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class SuspendMemberCommandValidator : AbstractValidator<SuspendMemberCommand>
{
    public SuspendMemberCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty();
    }
}
