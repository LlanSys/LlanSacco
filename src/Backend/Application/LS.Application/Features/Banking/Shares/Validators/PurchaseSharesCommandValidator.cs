using FluentValidation;
using LS.Application.Features.Banking.Shares.Commands;

namespace LS.Application.Features.Banking.Shares.Validators;

internal sealed class PurchaseSharesCommandValidator : AbstractValidator<PurchaseSharesCommand>
{
    public PurchaseSharesCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.ShareProductId).NotEmpty();
        RuleFor(request => request.Amount).GreaterThan(0);
    }
}
