using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class RejectMemberCommandValidator : AbstractValidator<RejectMemberCommand>
{
    public RejectMemberCommandValidator()
    {
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.MemberId).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.RejectionReason).NotEmpty().When(request => request.Request is not null);
    }
}
