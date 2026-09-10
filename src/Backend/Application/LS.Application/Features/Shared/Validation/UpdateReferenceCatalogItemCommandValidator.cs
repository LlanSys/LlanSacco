using LS.Application.Features.IAM.ReferenceData.Commands;
using LS.SharedKernel.Validation.Features.IAM.ReferenceData.Validators;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class UpdateReferenceCatalogItemCommandValidator : AbstractValidator<UpdateReferenceCatalogItemCommand>
{
    public UpdateReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Id).NotEmpty().WithMessage("Catalog item ID is required.");
        RuleFor(x => x.Request).SetValidator(new ReferenceCatalogItemRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

