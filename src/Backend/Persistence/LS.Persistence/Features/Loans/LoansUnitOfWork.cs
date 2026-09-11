using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.Loans.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Loans;

public class LoansUnitOfWork(
    LoansDBContext context,
    ILoanProductRepository loanProductRepository,
    ILoanApplicationRepository loanApplicationRepository,
    IRepository<LS.Domain.Features.Loans.Entities.LoanGuarantor> loanGuarantorRepository,
    IRepository<LS.Domain.Features.Loans.Entities.LoanRepaymentSchedule> loanRepaymentScheduleRepository,
    IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionCase> collectionCases,
    IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionAction> collectionActions,
    IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionPromise> collectionPromises,
    IPublisher publisher,
    ILogger<LoansUnitOfWork> logger
) : BaseUnitOfWork<LoansDBContext>(context, publisher, logger), ILoansUnitOfWork
{
    public ILoanProductRepository LoanProductRepository { get; } = loanProductRepository;
    public ILoanApplicationRepository LoanApplicationRepository { get; } = loanApplicationRepository;
    public IRepository<LS.Domain.Features.Loans.Entities.LoanGuarantor> LoanGuarantorRepository { get; } = loanGuarantorRepository;
    public IRepository<LS.Domain.Features.Loans.Entities.LoanRepaymentSchedule> LoanRepaymentScheduleRepository { get; } = loanRepaymentScheduleRepository;
    
    public IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionCase> CollectionCases { get; } = collectionCases;
    public IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionAction> CollectionActions { get; } = collectionActions;
    public IRepository<LS.Domain.Features.Loans.Collections.Entities.CollectionPromise> CollectionPromises { get; } = collectionPromises;
}
