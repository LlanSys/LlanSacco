using LS.Domain.Features.Banking.GuarantorReservations.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.GuarantorReservations.Entities;

public sealed class GuarantorReservation : BaseEntity, IRetainedFinancialRecord
{
    public Guid PledgeId { get; private set; }
    public Guid LoanId { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid SavingsAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string PolicyVersion { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public GuarantorReservationState State { get; private set; }
    public long Revision { get; private set; }
    public string? DecisionReason { get; private set; }

    public static GuarantorReservation Create(
        Guid tenantId,
        Guid pledgeId,
        Guid loanId,
        Guid memberId,
        Guid accountId,
        decimal amount,
        string currency,
        string policyVersion,
        DateTimeOffset expiresAt,
        string actor,
        DateTimeOffset now) => new()
        {
            TenantId = tenantId,
            PledgeId = pledgeId,
            LoanId = loanId,
            MemberId = memberId,
            SavingsAccountId = accountId,
            Amount = amount,
            Currency = currency,
            PolicyVersion = policyVersion,
            ExpiresAt = NormalizeExpiry(expiresAt),
            CreatedBy = actor,
            CreatedAt = now,
            State = GuarantorReservationState.Requested
        };

    public bool Matches(
        Guid loanId,
        Guid memberId,
        Guid accountId,
        decimal amount,
        string currency,
        string policyVersion,
        DateTimeOffset expiresAt) =>
        LoanId == loanId && MemberId == memberId && SavingsAccountId == accountId &&
        Amount == amount && string.Equals(Currency, currency, StringComparison.Ordinal) &&
        string.Equals(PolicyVersion, policyVersion, StringComparison.Ordinal) && ExpiresAt == NormalizeExpiry(expiresAt);

    private static DateTimeOffset NormalizeExpiry(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        return utc.AddTicks(-(utc.Ticks % 10));
    }

    public bool Hold(decimal available, bool eligible, DateTimeOffset now)
    {
        if (State != GuarantorReservationState.Requested)
        {
            return false;
        }

        if (!eligible || Amount <= 0 || Amount > available || ExpiresAt <= now)
        {
            State = GuarantorReservationState.Rejected;
            DecisionReason = "The account is ineligible, has insufficient available funds, or the request has expired.";
            Revision++;
            return false;
        }
        RemainingAmount = Amount;
        State = GuarantorReservationState.Held;
        Revision++;
        return true;
    }

    public bool CanCommit(DateTimeOffset now) =>
        State == GuarantorReservationState.Committed || State == GuarantorReservationState.Held && ExpiresAt > now;

    public void Commit(DateTimeOffset now)
    {
        if (!CanCommit(now))
        {
            throw new InvalidOperationException("Only a current hold can be committed.");
        }

        if (State == GuarantorReservationState.Committed)
        {
            return;
        }

        State = GuarantorReservationState.Committed;
        Revision++;
    }

    public decimal Release(bool committedReleaseAuthorized, bool expiry, DateTimeOffset now)
    {
        if (State == GuarantorReservationState.Committed && !committedReleaseAuthorized)
        {
            return 0;
        }

        if (expiry && (State != GuarantorReservationState.Held || ExpiresAt > now))
        {
            return 0;
        }

        if (State is GuarantorReservationState.Released or GuarantorReservationState.Expired)
        {
            return 0;
        }

        var released = RemainingAmount;
        RemainingAmount = 0;
        State = expiry ? GuarantorReservationState.Expired : GuarantorReservationState.Released;
        Revision++;
        return released;
    }

    public bool CanRecover(decimal amount) =>
        State == GuarantorReservationState.Committed && amount > 0 && amount <= RemainingAmount;

    public void AdoptCommitted(decimal unexplainedAmount)
    {
        if (State != GuarantorReservationState.Requested || Amount <= 0 || Amount > unexplainedAmount)
        {
            throw new InvalidOperationException("Legacy adoption requires an identified existing encumbrance.");
        }

        RemainingAmount = Amount;
        State = GuarantorReservationState.Committed;
        Revision++;
    }

    public void ReconcileReleasedLegacy(decimal unexplainedAmount)
    {
        if (State != GuarantorReservationState.Released || RemainingAmount != 0 || Amount <= 0 || unexplainedAmount < Amount)
        {
            throw new InvalidOperationException("Closed legacy security requires an identified unreconciled hold.");
        }

        Revision++;
    }

    public void Recover(decimal amount)
    {
        if (!CanRecover(amount))
        {
            throw new InvalidOperationException("Recovery exceeds committed coverage.");
        }

        RemainingAmount -= amount;
        Revision++;
    }
}
