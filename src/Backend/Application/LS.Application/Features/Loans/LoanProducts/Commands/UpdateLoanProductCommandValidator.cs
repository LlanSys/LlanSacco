using FluentValidation;

namespace LS.Application.Features.Loans.LoanProducts.Commands;

internal class UpdateLoanProductCommandValidator : AbstractValidator<UpdateLoanProductCommand>
{
    public UpdateLoanProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxTermInMonths).GreaterThan(0);
        RuleFor(x => x.MaxAmount).GreaterThan(0);
    }
}

