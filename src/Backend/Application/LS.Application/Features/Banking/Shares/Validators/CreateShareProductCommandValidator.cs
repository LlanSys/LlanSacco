using FluentValidation;
using LS.Application.Features.Banking.Shares.Commands;

namespace LS.Application.Features.Banking.Shares.Validators;

internal sealed class CreateShareProductCommandValidator : AbstractValidator<CreateShareProductCommand>
{
    public CreateShareProductCommandValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Code).NotEmpty();
        RuleFor(request => request.PricePerShare).GreaterThan(0);
        RuleFor(request => request.MinimumShares).GreaterThanOrEqualTo(0);
    }
}
