using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;

namespace LS.Application.Features.Accounting.Services;

public class LedgerService(IAccountingUnitOfWork uow, ICurrentTenantProvider tenantProvider) : ILedgerService
{
    public async Task<AppResponse<Guid>> PostJournalAsync(
        string referenceNumber,
        string description,
        DateTimeOffset transactionDate,
        IEnumerable<CreateJournalEntryDto> entries,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
        {
            return AppResponses.Failure<Guid>(AppError.Forbidden("A tenant is required to post a journal."));
        }

        ArgumentNullException.ThrowIfNull(entries);

        if (string.IsNullOrWhiteSpace(referenceNumber) || referenceNumber.Length > 100)
        {
            return Invalid(nameof(CreateJournalRequest.ReferenceNumber), "Provide a journal reference of at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > 500)
        {
            return Invalid(nameof(CreateJournalRequest.Description), "Provide a journal description of at most 500 characters.");
        }

        var lines = entries.ToList();
        var existing = await uow.JournalRepository.FirstOrDefaultAsync(
            q => q
                .Where(j => j.TenantId == tenantId && j.ReferenceNumber == referenceNumber)
                .Select(j => new Journal
                {
                    Id = j.Id,
                    CreatedBy = j.CreatedBy,
                    ReferenceNumber = j.ReferenceNumber,
                    Description = j.Description,
                    Status = j.Status,
                    TransactionDate = j.TransactionDate,
                    Lines = j.Lines.ToList()
                }),
            cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            var expected = lines.Select(e => (e.AccountId, e.BranchId, e.CostCenterId, e.Debit, e.Credit)).Order().ToArray();
            var actual = existing.Lines.Select(e => (e.AccountId, e.BranchId, e.CostCenterId, e.Debit, e.Credit)).Order().ToArray();

            if (existing.Status != JournalStatus.Posted
                || existing.TransactionDate.UtcTicks / TimeSpan.TicksPerMicrosecond != transactionDate.UtcTicks / TimeSpan.TicksPerMicrosecond
                || !actual.SequenceEqual(expected))
            {
                return AppResponses.Conflict<Guid>("The journal reference is already associated with different posting details.");
            }

            return AppResponses.Success(existing.Id);
        }

        if (lines.Any(line => line.Debit < 0 || line.Credit < 0))
        {
            return Invalid(nameof(CreateJournalRequest.Entries), "Debit and credit values cannot be negative.");
        }

        if (lines.Any(line => line.Debit > 0 && line.Credit > 0))
        {
            return Invalid(nameof(CreateJournalRequest.Entries), "A journal line cannot have both a debit and a credit.");
        }

        if (lines.Any(line => line.Description?.Length > 500))
        {
            return Invalid(nameof(CreateJournalRequest.Entries), "Journal line descriptions must have at most 500 characters.");
        }

        var totalDebit = lines.Sum(line => line.Debit);

        if (totalDebit != lines.Sum(line => line.Credit))
        {
            return AppResponses.Failure<Guid>(AppError.BusinessRule("Journal debits and credits must balance."));
        }

        if (totalDebit <= 0)
        {
            return AppResponses.Failure<Guid>(AppError.BusinessRule("A journal must have a positive non-zero value."));
        }

        var accountIds = lines.Select(line => line.AccountId).Distinct().ToArray();
        var accountsFound = await uow.AccountRepository.CountAsync(
            account => account.TenantId == tenantId && accountIds.Contains(account.Id), cancellationToken)
            .ConfigureAwait(false);

        if (accountsFound != accountIds.Length)
        {
            return AppResponses.NotFound<Guid>("One or more journal accounts could not be found.");
        }

        var journal = Journal.Create(tenantId, referenceNumber, description, transactionDate, createdBy);

        foreach (var entry in lines)
        {
            journal.Lines.Add(JournalLine.Create(
                tenantId,
                journal.Id,
                entry.AccountId,
                entry.Description,
                entry.Debit,
                entry.Credit,
                createdBy,
                entry.BranchId,
                entry.CostCenterId));
        }

        journal.Status = JournalStatus.Posted;
        await uow.JournalRepository.CreateAsync(journal, cancellationToken).ConfigureAwait(false);
        await uow.CompleteAsync(cancellationToken).ConfigureAwait(false);
        return AppResponses.Success(journal.Id);
    }

    private static AppResponse<Guid> Invalid(string field, string message)
        => AppResponses.Failure<Guid>(AppError.Validation(new Dictionary<string, string[]> { [field] = [message] }));
}
