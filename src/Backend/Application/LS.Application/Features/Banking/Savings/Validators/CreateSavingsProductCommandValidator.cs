using FluentValidation;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Validators;

internal class CreateSavingsProductCommandValidator : AbstractValidator<CreateSavingsProductCommand>
{
    public CreateSavingsProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MinimumBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.WithdrawalFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.DailyWithdrawalLimit).GreaterThanOrEqualTo(0).When(x => x.Request.DailyWithdrawalLimit.HasValue);
        RuleFor(x => x.Request.MonthlyWithdrawalLimit).GreaterThanOrEqualTo(0).When(x => x.Request.MonthlyWithdrawalLimit.HasValue);
    }
}
