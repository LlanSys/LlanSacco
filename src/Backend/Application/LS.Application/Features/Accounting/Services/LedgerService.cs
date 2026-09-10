using LS.Domain.Features.Accounting.Contracts;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Accounting.Services;

public class LedgerService(IAccountingUnitOfWork uow, ICurrentTenantProvider tenantProvider) : ILedgerService
{
    private readonly IAccountingUnitOfWork _uow = uow;
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;

    public async Task<Guid> PostJournalAsync(
        string referenceNumber,
        string description,
        DateTimeOffset transactionDate,
        IEnumerable<CreateJournalEntryDto> entries,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.TenantId;

        // Ensure double-entry validity
        var totalDebit = entries.Sum(e => e.Debit);
        var totalCredit = entries.Sum(e => e.Credit);

        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException($"Journal is unbalanced. Total Debits: {totalDebit}, Total Credits: {totalCredit}");
        }

        if (totalDebit <= 0)
        {
            throw new InvalidOperationException("Journal must have a positive non-zero value.");
        }

        var journal = Journal.Create(tenantId, referenceNumber, description, transactionDate, createdBy);

        foreach (var entry in entries)
        {
            if (entry.Debit < 0 || entry.Credit < 0)
            {
                throw new InvalidOperationException("Debit and Credit values cannot be negative.");
            }

            if (entry.Debit > 0 && entry.Credit > 0)
            {
                throw new InvalidOperationException("A single journal line cannot have both a debit and a credit.");
            }

            var line = JournalLine.Create(
                tenantId: tenantId,
                journalId: journal.Id,
                accountId: entry.AccountId,
                description: entry.Description,
                debit: entry.Debit,
                credit: entry.Credit,
                createdBy: createdBy,
                branchId: entry.BranchId,
                costCenterId: entry.CostCenterId);

            journal.Lines.Add(line);
        }

        journal.Status = JournalStatus.Posted;

        await _uow.JournalRepository.CreateAsync(journal, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        return journal.Id;
    }
}
