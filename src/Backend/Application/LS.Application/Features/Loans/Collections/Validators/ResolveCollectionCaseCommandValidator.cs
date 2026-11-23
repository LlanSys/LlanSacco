using FluentValidation;
using LS.Application.Features.Loans.Collections.Commands;

namespace LS.Application.Features.Loans.Collections.Validators;

internal sealed class ResolveCollectionCaseCommandValidator : AbstractValidator<ResolveCollectionCaseCommand>
{
    public ResolveCollectionCaseCommandValidator()
    {
        RuleFor(request => request.CollectionCaseId).NotEmpty();
        RuleFor(request => request.ResolutionReason).NotEmpty();
    }
}
