using FluentValidation;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Validators;

internal class OverTheCounterTransactionCommandValidator : AbstractValidator<OverTheCounterTransactionCommand>
{
    public OverTheCounterTransactionCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.FosaAccountId).NotEmpty();
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).NotEmpty();
    }
}
