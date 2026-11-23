using FluentValidation;
using LS.Application.Features.Loans.Collections.Commands;

namespace LS.Application.Features.Loans.Collections.Validators;

internal sealed class OpenCollectionCaseCommandValidator : AbstractValidator<OpenCollectionCaseCommand>
{
    public OpenCollectionCaseCommandValidator()
    {
        RuleFor(request => request.LoanApplicationId).NotEmpty();
        RuleFor(request => request.TotalArrearsAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DaysPastDue).GreaterThanOrEqualTo(0);
    }
}
