using FluentValidation;
using LS.Application.Features.Dividends.Commands;

namespace LS.Application.Features.Dividends.Validators;

internal sealed class RunDividendCalculationCommandValidator : AbstractValidator<RunDividendCalculationCommand>
{
    public RunDividendCalculationCommandValidator()
    {
        RuleFor(request => request.DeclarationId).NotEmpty();
    }
}
