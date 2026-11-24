using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class CancelGuaranteedLoanCommandValidator : AbstractValidator<CancelGuaranteedLoanCommand>
{
    public CancelGuaranteedLoanCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}
