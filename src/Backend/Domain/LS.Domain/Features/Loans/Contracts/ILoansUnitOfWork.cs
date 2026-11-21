using LS.Domain.Features.Loans.Collections.Contracts.Repositories;
using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Shared.Contracts;

namespace LS.Domain.Features.Loans.Contracts;

public interface ILoansUnitOfWork : ITransactionalUnitOfWork
{
    ILoanProductRepository LoanProductRepository { get; }
    ILoanApplicationRepository LoanApplicationRepository { get; }
    ILoanRepaymentRepository LoanRepaymentRepository { get; }
    ILoanGuarantorRepository LoanGuarantorRepository { get; }
    ILoanRepaymentScheduleRepository LoanRepaymentScheduleRepository { get; }
    
    // Collections
    ICollectionCaseRepository CollectionCaseRepository { get; }
    ICollectionActionRepository CollectionActionRepository { get; }
    ICollectionPromiseRepository CollectionPromiseRepository { get; }
}
