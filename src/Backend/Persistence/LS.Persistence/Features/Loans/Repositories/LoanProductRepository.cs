using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Features.Loans.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Loans.DataContext;

namespace LS.Persistence.Features.Loans.Repositories;

internal sealed class LoanProductRepository(LoansDBContext dbContext) : Repository<LoanProduct>(dbContext), ILoanProductRepository
{
}
