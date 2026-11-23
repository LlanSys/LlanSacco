using FluentValidation;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.SharedKernel.Features.Banking.Savings.Events;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Validators;

internal class WithdrawSavingsCommandValidator : AbstractValidator<WithdrawSavingsCommand>
{
    public WithdrawSavingsCommandValidator()
    {
        RuleFor(x => x.Request.MemberId).NotEmpty();
        RuleFor(x => x.Request.SavingsProductId).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Notes).MaximumLength(500);
        RuleFor(x => x.Request.ExternalReferenceId).MaximumLength(100);
    }
}
