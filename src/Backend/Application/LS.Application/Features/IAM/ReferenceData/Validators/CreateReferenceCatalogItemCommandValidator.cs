using FluentValidation;
using LS.Application.Features.IAM.ReferenceData.Commands;
using LS.SharedKernel.Validation.Features.IAM.ReferenceData.Validators;

namespace LS.Application.Features.IAM.ReferenceData.Validators;

internal sealed class CreateReferenceCatalogItemCommandValidator : AbstractValidator<CreateReferenceCatalogItemCommand>
{
    public CreateReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Request).SetValidator(new ReferenceCatalogItemRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
