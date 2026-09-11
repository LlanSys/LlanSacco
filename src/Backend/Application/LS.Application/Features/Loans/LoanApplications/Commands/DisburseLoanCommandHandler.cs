using LS.SharedKernel.Dtos.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

internal sealed partial class DisburseLoanCommandHandler(
    ILoansUnitOfWork unitOfWork,
    IEnumerable<IAmortizationStrategy> amortizationStrategies,
    ICurrentActorProvider actorProvider,
    IPublisher publisher,
    ILogger<DisburseLoanCommandHandler> logger)
    : IRequestHandler<DisburseLoanCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken).ConfigureAwait(false);
        if (loan == null)
        {
            return AppResponses.Failure<bool>("Loan application not found.");
        }

        if (loan.Status != LoanStatus.Approved)
        {
            return AppResponses.Failure<bool>($"Cannot disburse loan in status {loan.Status}. Must be Approved.");
        }

        loan.Status = LoanStatus.Disbursed;
        loan.DisbursementDate = DateTimeOffset.UtcNow;
        loan.UpdatedBy = actorProvider.ActorId.ToString();
        loan.UpdatedAt = DateTimeOffset.UtcNow;

        // Generate schedules
        var strategy = amortizationStrategies.FirstOrDefault(s => s.SupportedMethod == loan.InterestMethod);
        if (strategy == null)
        {
            return AppResponses.Failure<bool>($"No amortization strategy found for interest method {loan.InterestMethod}.");
        }

        var schedules = strategy.GenerateSchedule(loan);
        foreach (var schedule in schedules)
        {
            await unitOfWork.LoanRepaymentScheduleRepository.CreateAsync(schedule, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken).ConfigureAwait(false);
        
        var disbursedEvent = new LoanDisbursedIntegrationEvent(
            TenantId: loan.TenantId,
            LoanApplicationId: loan.Id,
            MemberId: loan.MemberId,
            Amount: loan.PrincipalAmount,
            FeeComponent: 0m, // Compute application fee if applicable
            LoanNumber: loan.ApplicationNumber,
            CreatedBy: actorProvider.ActorId.ToString(),
            BranchId: request.BranchId,
            CostCenterId: request.CostCenterId,
            PaymentChannelGlAccountId: request.PaymentChannelGlAccountId
        );

        await publisher.Publish(disbursedEvent, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogLoanDisbursed(logger, loan.Id, loan.ApplicationNumber);

        return AppResponses.Success(true);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Loan {ApplicationNumber} ({Id}) was disbursed.")]
    private static partial void LogLoanDisbursed(ILogger logger, Guid id, string applicationNumber);
}
