using FluentValidation;
using LS.SharedKernel.Features.Membership.Dtos;

namespace LS.SharedKernel.Validation.Features.Membership;

public class ApproveMemberRequestValidator : AbstractValidator<ApproveMemberRequest>
{
    public ApproveMemberRequestValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(x => x.ApprovalNote)
            .NotEmpty().WithMessage("Approval note is required.")
            .MaximumLength(500).WithMessage("Approval note cannot exceed 500 characters.");
    }
}
