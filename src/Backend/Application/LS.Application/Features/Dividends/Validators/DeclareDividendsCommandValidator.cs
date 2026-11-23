using FluentValidation;
using LS.Application.Features.Dividends.Commands;
using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Dividends.Validators;

internal class DeclareDividendsCommandValidator : AbstractValidator<DeclareDividendsCommand>
{
    public DeclareDividendsCommandValidator()
    {
        RuleFor(x => x.FinancialYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.ShareDividendRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DepositInterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShareWhtRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DepositWhtRate).GreaterThanOrEqualTo(0);
    }
}
