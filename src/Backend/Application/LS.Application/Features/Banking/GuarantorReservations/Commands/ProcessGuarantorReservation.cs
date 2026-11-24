using System.Text.Json;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using LS.Domain.Features.Banking.GuarantorReservations.Enums;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Banking.GuarantorReservations.Commands;

public sealed record ProcessGuarantorReservationCommand(GuarantorReservationInstruction Instruction)
    : IRequest<AppResponse<GuarantorReservationOutcome>>;

internal sealed class ProcessGuarantorReservationCommandHandler(
    IBankingUnitOfWork unitOfWork, ICurrentTenantProvider tenant,
    IContextEventPublisher<IBankingUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<ProcessGuarantorReservationCommand, AppResponse<GuarantorReservationOutcome>>
{
    public Task<AppResponse<GuarantorReservationOutcome>> Handle(ProcessGuarantorReservationCommand request, CancellationToken ct)
    {
        var instruction = request.Instruction;

        if (tenant.TenantId == Guid.Empty || instruction.TenantId != tenant.TenantId)
        {
            return Task.FromResult(AppResponses.Failure<GuarantorReservationOutcome>(AppError.Forbidden("The reservation tenant is invalid.")));
        }

        return unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                await unitOfWork.GuarantorReservationRepository.LockTenantAsync(tenant.TenantId, ct).ConfigureAwait(false);
                var operation = Enum.Parse<GuarantorReservationOperation>(instruction.Operation, ignoreCase: true);
                var canonical = instruction with { Operation = operation.ToString(), Items = instruction.Items.OrderBy(x => x.PledgeId).ToArray() };
                var fingerprint = GuarantorReservationFingerprint.Compute(canonical);
                var prior = await unitOfWork.GuarantorReservationReceiptRepository.FirstOrDefaultAsync(x => x.OperationId == instruction.OperationId, ct).ConfigureAwait(false);

                if (prior is not null)
                {
                    if (!string.Equals(prior.Fingerprint, fingerprint, StringComparison.Ordinal))
                    {
                        return AppResponses.Failure<GuarantorReservationOutcome>(AppError.Conflict("This operation identity has different reservation details."));
                    }

                    var recorded = JsonSerializer.Deserialize<GuarantorReservationOutcome>(prior.OutcomeJson)
                        ?? throw new InvalidOperationException("The stored reservation outcome is invalid.");

                    await publisher.PublishAsync(recorded, ct).ConfigureAwait(false);
                    return AppResponses.Success(recorded);
                }

                var outcome = await ProcessAsync(canonical, operation, ct).ConfigureAwait(false);
                await unitOfWork.GuarantorReservationReceiptRepository.CreateAsync(new GuarantorReservationReceipt
                {
                    TenantId = tenant.TenantId,
                    OperationId = instruction.OperationId,
                    LoanId = instruction.LoanId,
                    Fingerprint = fingerprint,
                    OutcomeJson = JsonSerializer.Serialize(outcome),
                    CreatedBy = instruction.RequestedBy,
                    CreatedAt = clock.GetUtcNow()
                }, ct)
                    .ConfigureAwait(false);

                await publisher.PublishAsync(outcome, ct).ConfigureAwait(false);
                return AppResponses.Success(outcome);
            },
            cancellationToken: ct);
    }

    private async Task<GuarantorReservationOutcome> ProcessAsync(
        GuarantorReservationInstruction instruction,
        GuarantorReservationOperation operation,
        CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var pledgeIds = instruction.Items.Select(x => x.PledgeId).ToArray();
        var existing = await unitOfWork.GuarantorReservationRepository.ListAsync(q => q.Where(x => pledgeIds.Contains(x.PledgeId)), ct).ConfigureAwait(false);
        var reservations = existing.ToDictionary(x => x.PledgeId);
        var accountIds = instruction.Items.Select(x => x.SavingsAccountId).Distinct().ToArray();
        var accounts = (await unitOfWork.SavingsAccountRepository.ListAsync(q => q.Where(x => accountIds.Contains(x.Id)), ct).ConfigureAwait(false)).ToDictionary(x => x.Id);
        var productIds = accounts.Values.Select(x => x.SavingsProductId).Distinct().ToArray();
        var products = (await unitOfWork.SavingsProductRepository.ListAsync(q => q.Where(x => productIds.Contains(x.Id)), ct).ConfigureAwait(false)).ToDictionary(x => x.Id);
        var activeHolds = await unitOfWork.GuarantorReservationRepository.ListAsync(
            q => q.Where(x => accountIds.Contains(x.SavingsAccountId) &&
                (x.State == GuarantorReservationState.Held || x.State == GuarantorReservationState.Committed)),
            ct)
            .ConfigureAwait(false);

        var explained = activeHolds.GroupBy(x => x.SavingsAccountId).ToDictionary(x => x.Key, x => x.Sum(y => y.RemainingAmount));

        GuarantorReservationOutcome Outcome(bool success, string? reason) => new(
            instruction.TenantId,
            instruction.OperationId,
            instruction.LoanId,
            operation.ToString(),
            success,
            reason,
            reservations.Values.OrderBy(x => x.PledgeId).Select(x => new GuarantorReservationOutcomeItem(
                x.PledgeId, x.SavingsAccountId, x.Amount, x.RemainingAmount, x.State.ToString(), x.Revision))
                .ToArray());

        foreach (var item in instruction.Items)
        {
            if (reservations.TryGetValue(item.PledgeId, out var reservation) && !reservation.Matches(
                instruction.LoanId,
                item.MemberId,
                item.SavingsAccountId,
                item.Amount,
                item.Currency,
                item.PolicyVersion,
                item.ExpiresAt))
            {
                return Outcome(false, "A pledge identity has different reservation details.");
            }
        }

        if (operation == GuarantorReservationOperation.Commit)
        {
            if (instruction.Items.Count < 2 || instruction.Items.Select(x => x.MemberId).Distinct().Count() < 2 ||
                instruction.RequiredCoverage <= 0 || existing.Count != instruction.Items.Count ||
                existing.Any(x => !x.CanCommit(now)) || existing.Sum(x => x.RemainingAmount) < instruction.RequiredCoverage)
            {
                return Outcome(false, "Current confirmed guarantor coverage is insufficient for disbursement.");
            }
        }

        if (operation is GuarantorReservationOperation.Recover or GuarantorReservationOperation.AdoptLegacy)
        {
            if (string.IsNullOrWhiteSpace(instruction.ApprovedBy) || string.Equals(instruction.RequestedBy, instruction.ApprovedBy, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(instruction.EvidenceReference))
            {
                return Outcome(false, "Independent approval and supporting evidence are required.");
            }
        }

        // Validate the whole batch before any mutation. Expected failures must not commit a partial recovery/release.
        foreach (var item in instruction.Items)
        {
            accounts.TryGetValue(item.SavingsAccountId, out var account);
            products.TryGetValue(account?.SavingsProductId ?? Guid.Empty, out var product);
            reservations.TryGetValue(item.PledgeId, out var reservation);

            if (operation == GuarantorReservationOperation.Commit && (account is null || !account.IsActive || product is null || !product.IsActive || !product.AllowsGuarantorReservations ||
                account.LockedFunds != explained.GetValueOrDefault(account.Id) || account.Balance - product.MinimumBalance < account.LockedFunds))
            {
                return Outcome(false, "The guarantor account requires reconciliation or is inactive.");
            }

            if (operation == GuarantorReservationOperation.Recover && (reservation is null || !reservation.CanRecover(item.RecoveryAmount) ||
                account is null || account.MemberId != item.MemberId || !account.IsActive || product is null || !product.IsActive ||
                account.LockedFunds != explained.GetValueOrDefault(account.Id) || account.Balance - product.MinimumBalance < item.RecoveryAmount))
            {
                return Outcome(false, "The approved recovery cannot be covered by the committed reservation.");
            }

            if (operation == GuarantorReservationOperation.Release && reservation?.State == GuarantorReservationState.Committed && !instruction.ReleaseCommitted)
            {
                return Outcome(false, "Committed security cannot be cancelled before an authorized settlement.");
            }

            if (operation == GuarantorReservationOperation.AdoptLegacy && ((reservation is not null &&
                !(instruction.ReleaseCommitted && reservation.State == GuarantorReservationState.Released)) || account is null || account.MemberId != item.MemberId ||
                account.LockedFunds - explained.GetValueOrDefault(account.Id) < instruction.Items.Where(x => x.SavingsAccountId == account.Id).Sum(x => x.Amount)))
            {
                return Outcome(false, "Legacy adoption exceeds the unexplained locked balance or the pledge already exists.");
            }
        }

        var succeeded = true;
        string? reason = null;

        foreach (var item in instruction.Items)
        {
            accounts.TryGetValue(item.SavingsAccountId, out var account);
            products.TryGetValue(account?.SavingsProductId ?? Guid.Empty, out var product);

            if (!reservations.TryGetValue(item.PledgeId, out var reservation))
            {
                if (operation is GuarantorReservationOperation.Commit or GuarantorReservationOperation.Recover or GuarantorReservationOperation.Expire)
                {
                    continue;
                }

                reservation = GuarantorReservation.Create(
                    instruction.TenantId,
                    item.PledgeId,
                    instruction.LoanId,
                    item.MemberId,
                    item.SavingsAccountId,
                    item.Amount,
                    item.Currency,
                    item.PolicyVersion,
                    item.ExpiresAt,
                    instruction.RequestedBy,
                    now);

                await unitOfWork.GuarantorReservationRepository.CreateAsync(reservation, ct).ConfigureAwait(false);
                reservations.Add(item.PledgeId, reservation);
            }

            switch (operation)
            {
                case GuarantorReservationOperation.Reserve:
                    if (reservation.State != GuarantorReservationState.Requested)
                    {
                        succeeded &= reservation.State is GuarantorReservationState.Held or GuarantorReservationState.Committed;
                        break;
                    }
                    var eligible = account is not null && account.MemberId == item.MemberId && account.IsActive && product?.IsActive == true && product.AllowsGuarantorReservations &&
                        account.LockedFunds == explained.GetValueOrDefault(account.Id);
                    if (reservation.Hold(account?.GetAvailableBalance(product?.MinimumBalance ?? 0) ?? 0, eligible, now))
                    {
                        account!.LockedFunds += item.Amount;
                        explained[account.Id] = explained.GetValueOrDefault(account.Id) + item.Amount;
                    }
                    else
                    {
                        succeeded = false;
                        reason = reservation.DecisionReason;
                    }
                    break;
                case GuarantorReservationOperation.Commit:
                    reservation.Commit(now);
                    break;
                case GuarantorReservationOperation.Release:
                case GuarantorReservationOperation.Expire:
                    var released = reservation.Release(instruction.ReleaseCommitted, operation == GuarantorReservationOperation.Expire, now);
                    if (released > 0)
                    {
                        if (account is null || account.LockedFunds < released)
                        {
                            throw new InvalidOperationException("A reservation release requires account reconciliation.");
                        }

                        account.LockedFunds -= released;
                    }
                    break;
                case GuarantorReservationOperation.Recover:
                    reservation.Recover(item.RecoveryAmount);
                    account!.LockedFunds -= item.RecoveryAmount;
                    account.Balance -= item.RecoveryAmount;
                    var reference = "GR-" + Convert.ToBase64String(instruction.OperationId.ToByteArray().Concat(item.PledgeId.ToByteArray()).ToArray())
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
                    var transaction = SavingsTransaction.Create(
                        account.Id,
                        SavingsTransactionType.GuarantorRecovery,
                        item.RecoveryAmount,
                        "Approved guarantor principal recovery",
                        reference,
                        instruction.ApprovedBy!);
                    transaction.GuarantorOperationId = instruction.OperationId;
                    transaction.GuarantorPledgeId = item.PledgeId;
                    transaction.CreatedAt = now;
                    await unitOfWork.SavingsTransactionRepository.CreateAsync(transaction, ct).ConfigureAwait(false);
                    break;
                case GuarantorReservationOperation.AdoptLegacy:
                    if (reservation.State == GuarantorReservationState.Released)
                    {
                        reservation.ReconcileReleasedLegacy(account!.LockedFunds - explained.GetValueOrDefault(account.Id));
                        account.LockedFunds -= item.Amount;
                        break;
                    }
                    reservation.AdoptCommitted(account!.LockedFunds - explained.GetValueOrDefault(account.Id));
                    explained[account.Id] = explained.GetValueOrDefault(account.Id) + item.Amount;
                    if (instruction.ReleaseCommitted)
                    {
                        account.LockedFunds -= reservation.Release(true, false, now);
                        explained[account.Id] -= item.Amount;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported reservation operation.");
            }
            reservation.UpdatedBy = instruction.ApprovedBy ?? instruction.RequestedBy;
            reservation.UpdatedAt = now;

            if (existing.Any(x => x.Id == reservation.Id))
            {
                await unitOfWork.GuarantorReservationRepository.UpdateAsync(reservation, ct).ConfigureAwait(false);
            }
        }

        foreach (var account in accounts.Values.OrderBy(x => x.Id))
        {
            await unitOfWork.SavingsAccountRepository.UpdateAsync(account, ct).ConfigureAwait(false);
        }

        return Outcome(succeeded, reason);
    }
}
