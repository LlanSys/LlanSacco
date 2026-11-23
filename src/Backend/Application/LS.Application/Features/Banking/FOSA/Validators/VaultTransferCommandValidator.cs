using FluentValidation;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Features.Banking.FOSA.ValueObjects;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Validators;

internal class VaultTransferCommandValidator : AbstractValidator<VaultTransferCommand>
{
    public VaultTransferCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.VaultId).NotEmpty();
        RuleFor(x => x.Breakdown).NotNull();
    }
}
