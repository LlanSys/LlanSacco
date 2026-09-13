using FluentValidation;
using System;

namespace LS.Application.Features.Banking.Deposits.Commands;

internal class WithdrawFromDepositCommandValidator : AbstractValidator<WithdrawFromDepositCommand>
{
    public WithdrawFromDepositCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Reference).MaximumLength(100);
    }
}

