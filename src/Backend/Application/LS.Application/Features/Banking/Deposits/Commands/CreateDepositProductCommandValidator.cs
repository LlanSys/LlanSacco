using FluentValidation;
using LS.Domain.Features.Banking.Deposits.Enums;
using System;

namespace LS.Application.Features.Banking.Deposits.Commands;

internal class CreateDepositProductCommandValidator : AbstractValidator<CreateDepositProductCommand>
{
    public CreateDepositProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Type).IsEnumName(typeof(DepositType), caseSensitive: false);
        RuleFor(x => x.Request.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.TermMonths).GreaterThan(0);
        RuleFor(x => x.Request.MinimumDeposit).GreaterThanOrEqualTo(0);
        
        RuleFor(x => x.Request.PenaltyStrategy).IsEnumName(typeof(EarlyWithdrawalPenaltyStrategy), caseSensitive: false);
        
        When(x => Enum.TryParse<EarlyWithdrawalPenaltyStrategy>(x.Request.PenaltyStrategy, true, out var strategy) && strategy == EarlyWithdrawalPenaltyStrategy.FlatPercentage, () => {
            RuleFor(x => x.Request.FlatPenaltyRate).NotNull().GreaterThanOrEqualTo(0);
        });
        
        When(x => Enum.TryParse<EarlyWithdrawalPenaltyStrategy>(x.Request.PenaltyStrategy, true, out var strategy) && strategy == EarlyWithdrawalPenaltyStrategy.InterestForfeiture, () => {
            RuleFor(x => x.Request.InterestForfeiturePercentage).NotNull().InclusiveBetween(0, 100);
        });

        When(x => Enum.TryParse<EarlyWithdrawalPenaltyStrategy>(x.Request.PenaltyStrategy, true, out var strategy) && strategy == EarlyWithdrawalPenaltyStrategy.ProRataInterestReduction, () => {
            RuleFor(x => x.Request.ProRataReducedInterestRate).NotNull().GreaterThanOrEqualTo(0);
        });
    }
}

