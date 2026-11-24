using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class ApproveGuarantorSecurityReviewCommandValidator : AbstractValidator<ApproveGuarantorSecurityReviewCommand>
{
    public ApproveGuarantorSecurityReviewCommandValidator() => RuleFor(x => x.ReviewId).NotEmpty();
}
