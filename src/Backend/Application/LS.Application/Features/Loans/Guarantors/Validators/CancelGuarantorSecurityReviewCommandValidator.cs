using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class CancelGuarantorSecurityReviewCommandValidator : AbstractValidator<CancelGuarantorSecurityReviewCommand>
{
    public CancelGuarantorSecurityReviewCommandValidator() => RuleFor(x => x.ReviewId).NotEmpty();
}
