using FluentValidation;
using LS.Application.Features.Loans.Collections.Commands;

namespace LS.Application.Features.Loans.Collections.Validators;

internal sealed class AssignCollectionOfficerCommandValidator : AbstractValidator<AssignCollectionOfficerCommand>
{
    public AssignCollectionOfficerCommandValidator()
    {
        RuleFor(request => request.CollectionCaseId).NotEmpty();
        RuleFor(request => request.OfficerUserId).NotEmpty();
    }
}
