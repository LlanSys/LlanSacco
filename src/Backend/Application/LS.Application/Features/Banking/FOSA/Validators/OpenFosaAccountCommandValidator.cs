using FluentValidation;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.FOSA.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.FOSA.Validators;

internal class OpenFosaAccountCommandValidator : AbstractValidator<OpenFosaAccountCommand>
{
    public OpenFosaAccountCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
    }
}
