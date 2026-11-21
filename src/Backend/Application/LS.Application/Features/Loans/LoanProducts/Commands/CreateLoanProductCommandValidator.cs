using FluentValidation;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

internal class CreateLoanProductCommandValidator : AbstractValidator<CreateLoanProductCommand>
{
    public CreateLoanProductCommandValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxTermInMonths).GreaterThan(0);
        RuleFor(x => x.MaxAmount).GreaterThan(0);
    }
}

