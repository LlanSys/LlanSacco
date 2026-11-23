using FluentValidation;
using LS.Application.Features.Dividends.Commands;

namespace LS.Application.Features.Dividends.Validators;

internal sealed class ApproveDividendDeclarationCommandValidator : AbstractValidator<ApproveDividendDeclarationCommand>
{
    public ApproveDividendDeclarationCommandValidator()
    {
        RuleFor(request => request.DeclarationId).NotEmpty();
    }
}
