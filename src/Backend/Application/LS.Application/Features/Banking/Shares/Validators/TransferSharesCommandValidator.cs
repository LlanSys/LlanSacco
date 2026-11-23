using FluentValidation;
using LS.Application.Features.Banking.Shares.Commands;

namespace LS.Application.Features.Banking.Shares.Validators;

internal sealed class TransferSharesCommandValidator : AbstractValidator<TransferSharesCommand>
{
    public TransferSharesCommandValidator()
    {
        RuleFor(request => request.FromMemberId).NotEmpty();
        RuleFor(request => request.ToMemberId).NotEmpty();
        RuleFor(request => request.ShareProductId).NotEmpty();
        RuleFor(request => request.NumberOfShares).GreaterThan(0);
    }
}
