using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class VerifyMemberKycCommandValidator : AbstractValidator<VerifyMemberKycCommand>
{
    public VerifyMemberKycCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
    }
}
