using FluentValidation;
using LS.Application.Features.Banking.GuarantorReservations.Commands;
using LS.Domain.Features.Banking.GuarantorReservations.Enums;

namespace LS.Application.Features.Banking.GuarantorReservations.Validators;

internal sealed class ProcessGuarantorReservationCommandValidator : AbstractValidator<ProcessGuarantorReservationCommand>
{
    public ProcessGuarantorReservationCommandValidator()
    {
        RuleFor(x => x.Instruction).NotNull();
        When(
            x => x.Instruction is not null,
            () =>
            {
                RuleFor(x => x.Instruction.TenantId).NotEmpty();
                RuleFor(x => x.Instruction.OperationId).NotEmpty();
                RuleFor(x => x.Instruction.FormatVersion).Equal(1);
                RuleFor(x => x.Instruction.LoanId).NotEmpty();
                RuleFor(x => x.Instruction.RequestedBy).NotEmpty();
                RuleFor(x => x.Instruction.Operation).Must(value => Enum.TryParse<GuarantorReservationOperation>(value, true, out var operation) && Enum.IsDefined(operation));
                RuleFor(x => x.Instruction.Items).Cascade(CascadeMode.Stop).NotEmpty().Must(items => items.Count <= 100 && items.Select(x => x.PledgeId).Distinct().Count() == items.Count);
                RuleFor(x => x.Instruction.Items).Must(items => items.Count == 1)
                    .When(x => x.Instruction.Items is not null && string.Equals(x.Instruction.Operation, "Reserve", StringComparison.OrdinalIgnoreCase));

                RuleForEach(x => x.Instruction.Items).ChildRules(item =>
                {
                    item.RuleFor(x => x.PledgeId).NotEmpty();
                    item.RuleFor(x => x.MemberId).NotEmpty();
                    item.RuleFor(x => x.SavingsAccountId).NotEmpty();
                    item.RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
                    item.RuleFor(x => x.RecoveryAmount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
                    item.RuleFor(x => x.Currency).Equal("KES");
                    item.RuleFor(x => x.PolicyVersion).NotEmpty().MaximumLength(80);
                    item.RuleFor(x => x.ExpiresAt).NotEqual(default(DateTimeOffset));
                });
            });
    }
}
