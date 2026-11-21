using LS.Domain.Features.Loans.Collections.Contracts.Repositories;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.Loans.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Loans;

public class LoansUnitOfWork(
    LoansDBContext context,
    ILoanProductRepository loanProductRepository,
    ILoanApplicationRepository loanApplicationRepository,
    ILoanRepaymentRepository loanRepaymentRepository,
    ILoanGuarantorRepository loanGuarantorRepository,
    ILoanRepaymentScheduleRepository loanRepaymentScheduleRepository,
    ICollectionCaseRepository collectionCaseRepository,
    ICollectionActionRepository collectionActionRepository,
    ICollectionPromiseRepository collectionPromiseRepository,
    IPublisher publisher,
    ILogger<LoansUnitOfWork> logger
) : BaseUnitOfWork<LoansDBContext>(context, publisher, logger), ILoansUnitOfWork
{
    public ILoanProductRepository LoanProductRepository { get; } = loanProductRepository;
    public ILoanApplicationRepository LoanApplicationRepository { get; } = loanApplicationRepository;
    public ILoanRepaymentRepository LoanRepaymentRepository { get; } = loanRepaymentRepository;
    public ILoanGuarantorRepository LoanGuarantorRepository { get; } = loanGuarantorRepository;
    public ILoanRepaymentScheduleRepository LoanRepaymentScheduleRepository { get; } = loanRepaymentScheduleRepository;
    
    public ICollectionCaseRepository CollectionCaseRepository { get; } = collectionCaseRepository;
    public ICollectionActionRepository CollectionActionRepository { get; } = collectionActionRepository;
    public ICollectionPromiseRepository CollectionPromiseRepository { get; } = collectionPromiseRepository;
}
