using FluentValidation;
using LS.Application.Features.Dividends.Commands;

namespace LS.Application.Features.Dividends.Validators;

internal sealed class PostDividendDistributionsCommandValidator : AbstractValidator<PostDividendDistributionsCommand>
{
    public PostDividendDistributionsCommandValidator()
    {
        RuleFor(request => request.DeclarationId).NotEmpty();
    }
}
