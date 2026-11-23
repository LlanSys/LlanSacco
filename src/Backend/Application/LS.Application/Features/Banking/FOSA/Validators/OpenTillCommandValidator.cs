using FluentValidation;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Enums;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Validators;

internal class OpenTillCommandValidator : AbstractValidator<OpenTillCommand>
{
    public OpenTillCommandValidator()
    {
        RuleFor(x => x.TellerTillId).NotEmpty();
        RuleFor(x => x.TellerUserId).NotEmpty();
    }
}
