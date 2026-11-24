using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class RequestGuarantorSecurityReviewCommandValidator : AbstractValidator<RequestGuarantorSecurityReviewCommand>
{
    public RequestGuarantorSecurityReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.LoanId).NotEmpty();
        RuleFor(x => x.PledgeId).NotEmpty();
        RuleFor(x => x.Kind).Must(x => x is "AdoptLegacy" or "Substitute");
        RuleFor(x => x.EvidenceReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SavingsAccountId).NotNull().NotEqual(Guid.Empty).When(x => x.Kind == "AdoptLegacy");
        RuleFor(x => x.ReplacementMemberId).NotNull().NotEqual(Guid.Empty).When(x => x.Kind == "Substitute");
    }
}
