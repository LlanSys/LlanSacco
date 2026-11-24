using FluentValidation;
using LS.Application.Features.Loans.LoanProducts.Commands;

namespace LS.Application.Features.Loans.LoanProducts.Validators;

internal class CreateLoanProductCommandValidator : AbstractValidator<CreateLoanProductCommand>
{
    public CreateLoanProductCommandValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.InterestRate).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxTermInMonths).InclusiveBetween(1, 600);
        RuleFor(x => x.MaxAmount).GreaterThan(0);
        RuleFor(x => x.GuarantorReservationDays).InclusiveBetween(1, 365);
        RuleFor(x => x.GuarantorConsentDays).InclusiveBetween(1, 365);
        RuleFor(x => x.GuarantorPolicyVersion).NotEmpty().MaximumLength(80);
        RuleFor(x => x.EligibleDepositMultiple).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
