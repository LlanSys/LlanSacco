using FluentValidation;

namespace LS.Application.Features.Loans.LoanRepayments.Commands;

internal class ProcessRepaymentCommandValidator : AbstractValidator<ProcessRepaymentCommand>
{
    public ProcessRepaymentCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReceiptNumber).NotEmpty();
    }
}

