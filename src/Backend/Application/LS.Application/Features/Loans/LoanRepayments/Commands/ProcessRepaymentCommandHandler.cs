using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanRepayments.Commands;

internal sealed partial class ProcessRepaymentCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    IContextEventPublisher<ILoansUnitOfWork> publisher,
    ILogger<ProcessRepaymentCommandHandler> logger)
    : IRequestHandler<ProcessRepaymentCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(ProcessRepaymentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        var actorId = actorProvider.ActorId;
        if (tenantId == Guid.Empty) return AppResponses.Failure<Guid>(AppError.Forbidden("A tenant is required."));
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.ReceiptNumber) || request.ReceiptNumber.Length > 50)
            return AppResponses.Failure<Guid>("A positive amount and valid receipt number are required.");
        var prior = await unitOfWork.LoanApplicationRepository.FirstOrDefaultAsync(q => q.Where(l => l.TenantId == tenantId)
            .SelectMany(l => l.Repayments).Where(r => r.ReceiptNumber == request.ReceiptNumber), cancellationToken);
        if (prior is not null)
            return prior.LoanApplicationId == request.LoanApplicationId && prior.TotalAmount == request.Amount
                ? AppResponses.Success(prior.Id)
                : AppResponses.Failure<Guid>("This receipt number is already associated with different repayment details.");

        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken).ConfigureAwait(false);
        if (loan == null || loan.TenantId != tenantId)
        {
            return AppResponses.Failure<Guid>("Loan application not found.");
        }

        if (loan.Status != LoanStatus.Disbursed && loan.Status != LoanStatus.Defaulted && loan.Status != LoanStatus.Active)
        {
            return AppResponses.Failure<Guid>("This loan is not eligible for repayment.");
        }

        if (request.Amount > loan.OutstandingPrincipal + loan.OutstandingInterest)
            return AppResponses.Failure<Guid>("Repayment exceeds the outstanding balance.");
        decimal remainingAmount = request.Amount;
        decimal penaltyComponent = 0; 
        decimal interestComponent = Math.Min(remainingAmount, loan.OutstandingInterest);
        remainingAmount -= interestComponent;

        decimal principalComponent = Math.Min(remainingAmount, loan.OutstandingPrincipal);
        remainingAmount -= principalComponent;
        
        var repayment = LoanRepayment.Create(
            tenantId: tenantId,
            loanApplicationId: loan.Id,
            receiptNumber: request.ReceiptNumber,
            totalAmount: request.Amount,
            principalComponent: principalComponent,
            interestComponent: interestComponent,
            penaltyComponent: penaltyComponent,
            createdBy: actorId.ToString()
        );

        loan.OutstandingInterest -= interestComponent;
        loan.OutstandingPrincipal -= principalComponent;

        if (loan.OutstandingPrincipal <= 0 && loan.OutstandingInterest <= 0)
        {
            loan.Status = LoanStatus.Closed;
            loan.UpdatedBy = actorId.ToString();
            loan.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await unitOfWork.LoanRepaymentRepository.CreateAsync(repayment, cancellationToken);

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken).ConfigureAwait(false);
        
        var repaymentEvent = new LoanRepaymentIntegrationEvent(
            TenantId: loan.TenantId,
            LoanApplicationId: loan.Id,
            RepaymentId: repayment.Id,
            Amount: repayment.TotalAmount,
            PrincipalComponent: repayment.PrincipalComponent,
            InterestComponent: repayment.InterestComponent,
            PenaltyComponent: repayment.PenaltyComponent,
            FeeComponent: 0m,
            ReceiptNumber: repayment.ReceiptNumber,
            CreatedBy: actorId.ToString(),
            BranchId: request.BranchId,
            CostCenterId: request.CostCenterId,
            PaymentChannelGlAccountId: request.PaymentChannelGlAccountId
        ) { EventId = repayment.Id, OccurredAt = repayment.PaymentDate };

        await publisher.PublishAsync(repaymentEvent, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogRepaymentProcessed(logger, repayment.Id, loan.ApplicationNumber, request.Amount);

        return AppResponses.Success(repayment.Id);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Repayment {Id} processed for Loan {ApplicationNumber}. Amount: {Amount}")]
    private static partial void LogRepaymentProcessed(ILogger logger, Guid id, string applicationNumber, decimal amount);
}
