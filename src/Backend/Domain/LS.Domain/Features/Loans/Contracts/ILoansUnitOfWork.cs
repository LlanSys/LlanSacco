using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.Loans.Contracts;

public interface ILoansUnitOfWork : ITransactionalUnitOfWork
{
    ILoanProductRepository LoanProductRepository { get; }
    ILoanApplicationRepository LoanApplicationRepository { get; }
    IRepository<Entities.LoanGuarantor> LoanGuarantorRepository { get; }
    IRepository<Entities.LoanRepaymentSchedule> LoanRepaymentScheduleRepository { get; }
    
    // Collections
    IRepository<Collections.Entities.CollectionCase> CollectionCases { get; }
    IRepository<Collections.Entities.CollectionAction> CollectionActions { get; }
    IRepository<Collections.Entities.CollectionPromise> CollectionPromises { get; }
}
