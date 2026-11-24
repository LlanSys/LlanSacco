using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Features.Loans.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.Loans.DataContext;

namespace LS.Persistence.Features.Loans.Repositories;

internal sealed class LoanApplicationRepository(LoansDBContext dbContext) : Repository<LoanApplication>(dbContext), ILoanApplicationRepository
{
    public Task LockBorrowerAsync(Guid memberId, CancellationToken cancellationToken)
    {
        if (dbContext.CurrentTenantId == Guid.Empty || memberId == Guid.Empty)
        {
            throw new InvalidOperationException("Borrowing-capacity checks require a tenant and borrower.");
        }

        return TransactionLock.AcquireAsync(dbContext, $"loan-capacity:{dbContext.CurrentTenantId:D}:{memberId:D}", cancellationToken);
    }
}
