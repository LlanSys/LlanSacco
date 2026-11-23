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

internal class SetDividendPreferenceCommandValidator : AbstractValidator<SetDividendPreferenceCommand>
{
    public SetDividendPreferenceCommandValidator()
    {
        RuleFor(x => x.CapitalizePercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.FosaPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.ExternalBankPercentage).InclusiveBetween(0, 100);

        RuleFor(x => x)
            .Must(x => x.CapitalizePercentage + x.FosaPercentage + x.ExternalBankPercentage == 100)
            .WithMessage("Total distribution preference must equal 100%");
    }
}
