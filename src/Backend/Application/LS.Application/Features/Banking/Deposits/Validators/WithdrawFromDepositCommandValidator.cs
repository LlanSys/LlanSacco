using FluentValidation;
using LS.Application.Features.Banking.Deposits.Commands;
using System;

namespace LS.Application.Features.Banking.Deposits.Validators;

internal class WithdrawFromDepositCommandValidator : AbstractValidator<WithdrawFromDepositCommand>
{
    public WithdrawFromDepositCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Reference).MaximumLength(100);
    }
}
