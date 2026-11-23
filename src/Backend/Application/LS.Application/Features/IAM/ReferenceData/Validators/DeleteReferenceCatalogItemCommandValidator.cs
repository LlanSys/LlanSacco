using FluentValidation;
using LS.Application.Features.IAM.ReferenceData.Commands;

namespace LS.Application.Features.IAM.ReferenceData.Validators;

internal sealed class DeleteReferenceCatalogItemCommandValidator : AbstractValidator<DeleteReferenceCatalogItemCommand>
{
    public DeleteReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Id).NotEmpty().WithMessage("Catalog item ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
