using LS.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Features.Accounting.Dtos;

namespace LS.Application.Features.Accounting.Contracts.Interfaces;

public interface ILedgerService
{
    /// <summary>
    /// Posts a double-entry journal ensuring debits equal credits.
    /// </summary>
    Task<AppResponse<Guid>> PostJournalAsync(
        string referenceNumber,
        string description,
        DateTimeOffset transactionDate,
        IEnumerable<CreateJournalEntryRequest> entries,
        string createdBy,
        CancellationToken cancellationToken = default);
}
