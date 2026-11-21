using LS.Application.Features.IAM.Menus.Commands;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class DeleteMenuCommandValidator : AbstractValidator<DeleteMenuCommand>
{
    public DeleteMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

