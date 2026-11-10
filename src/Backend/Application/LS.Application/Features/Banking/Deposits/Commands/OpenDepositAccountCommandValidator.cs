using FluentValidation;
using System;

namespace LS.Application.Features.Banking.Deposits.Commands;

internal class OpenDepositAccountCommandValidator : AbstractValidator<OpenDepositAccountCommand>
{
    public OpenDepositAccountCommandValidator()
    {
        RuleFor(x => x.Request.MemberId).NotEmpty();
        RuleFor(x => x.Request.DepositProductId).NotEmpty();
    }
}

