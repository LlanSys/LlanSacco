using FluentValidation;
using LS.SharedKernel.Features.Membership.Dtos;

namespace LS.SharedKernel.Validation.Features.Membership;

public class NominateGuarantorRequestValidator : AbstractValidator<NominateGuarantorRequest>
{
    public NominateGuarantorRequestValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(x => x.NominatedGuarantorMemberId)
            .NotEmpty().WithMessage("Guarantor Member ID is required.")
            .NotEqual(x => x.MemberId).WithMessage("A member cannot nominate themselves as a guarantor.");

        RuleFor(x => x.AmountToGuarantee)
            .GreaterThan(0).WithMessage("Guarantee amount must be greater than zero.");
    }
}
