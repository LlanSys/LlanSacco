using FluentValidation;
using LS.Application.Features.Loans.Collections.Commands;

namespace LS.Application.Features.Loans.Collections.Validators;

internal sealed class RecordPromiseToPayCommandValidator : AbstractValidator<RecordPromiseToPayCommand>
{
    public RecordPromiseToPayCommandValidator()
    {
        RuleFor(request => request.CollectionCaseId).NotEmpty();
        RuleFor(request => request.PromiseDate).NotEmpty();
        RuleFor(request => request.PromiseAmount).GreaterThan(0);
    }
}
