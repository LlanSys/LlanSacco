using LS.Domain.Features.Loans.Collections.Contracts.Repositories;
using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Shared.Contracts;
using LS.Domain.Features.Loans.Guarantors.Contracts.Repositories;

namespace LS.Domain.Features.Loans.Contracts;

public interface ILoansUnitOfWork : ITransactionalUnitOfWork
{
    IGuarantorRecoveryRepository GuarantorRecoveryRepository { get; }
    IGuarantorSecurityReviewRepository GuarantorSecurityReviewRepository { get; }
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
