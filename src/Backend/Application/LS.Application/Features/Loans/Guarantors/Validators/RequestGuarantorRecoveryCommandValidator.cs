using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class RequestGuarantorRecoveryCommandValidator : AbstractValidator<RequestGuarantorRecoveryCommand>
{
    public RequestGuarantorRecoveryCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty();
        RuleFor(x => x.RecoveryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.EvidenceReference).NotEmpty().MaximumLength(200);
    }
}
