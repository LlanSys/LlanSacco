using LS.Application.Features.IAM.Menus.Commands;
using LS.SharedKernel.Validation.Features.IAM.Menus.Validators;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

