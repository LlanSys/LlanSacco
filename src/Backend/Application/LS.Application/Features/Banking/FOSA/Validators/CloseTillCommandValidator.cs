using FluentValidation;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Features.Banking.FOSA.ValueObjects;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Validators;

internal class CloseTillCommandValidator : AbstractValidator<CloseTillCommand>
{
    public CloseTillCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.Breakdown).NotNull();
    }
}
