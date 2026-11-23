using FluentValidation;
using LS.Application.Features.Loans.LoanRepayments.Commands;

namespace LS.Application.Features.Loans.LoanRepayments.Validators;

internal class ProcessRepaymentCommandValidator : AbstractValidator<ProcessRepaymentCommand>
{
    public ProcessRepaymentCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReceiptNumber).NotEmpty();
    }
}
