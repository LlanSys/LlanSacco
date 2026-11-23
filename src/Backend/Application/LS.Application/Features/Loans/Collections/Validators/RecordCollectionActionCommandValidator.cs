using FluentValidation;
using LS.Application.Features.Loans.Collections.Commands;

namespace LS.Application.Features.Loans.Collections.Validators;

internal sealed class RecordCollectionActionCommandValidator : AbstractValidator<RecordCollectionActionCommand>
{
    public RecordCollectionActionCommandValidator()
    {
        RuleFor(request => request.CollectionCaseId).NotEmpty();
        RuleFor(request => request.ActionType).IsInEnum();
        RuleFor(request => request.Notes).NotEmpty();
    }
}
