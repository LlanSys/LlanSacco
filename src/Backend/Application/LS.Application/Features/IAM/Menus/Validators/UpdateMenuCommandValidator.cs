using FluentValidation;
using LS.Application.Features.IAM.Menus.Commands;
using LS.SharedKernel.Validation.Features.IAM.Menus.Validators;

namespace LS.Application.Features.IAM.Menus.Validators;

internal sealed class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
