using LS.SharedKernel.Dtos.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

internal sealed partial class AcceptGuarantorPledgeCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    IPublisher publisher,
    ILogger<AcceptGuarantorPledgeCommandHandler> logger)
    : IRequestHandler<AcceptGuarantorPledgeCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(AcceptGuarantorPledgeCommand request, CancellationToken cancellationToken)
    {
        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken).ConfigureAwait(false);
        if (loan == null)
        {
            return AppResponses.Failure<bool>("Loan application not found.");
        }

        // Check if the guarantor exists on the application
        var guarantor = loan.Guarantors.FirstOrDefault(g => g.GuarantorMemberId == request.GuarantorMemberId);
        if (guarantor == null)
        {
             return AppResponses.Failure<bool>("Guarantor not found on this loan application.");
        }

        if (guarantor.Status == GuarantorStatus.Accepted)
        {
             return AppResponses.Failure<bool>("Guarantor pledge is already accepted.");
        }

        if (request.AcceptedAmount <= 0)
        {
            return AppResponses.Failure<bool>("Accepted amount must be greater than zero.");
        }

        guarantor.Status = GuarantorStatus.Accepted;
        guarantor.LockedAmount = request.AcceptedAmount;
        guarantor.UpdatedBy = actorProvider.ActorId.ToString();
        guarantor.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken).ConfigureAwait(false);

        // Publish integration event to lock savings funds
        var guarantorAcceptedEvent = new GuarantorAcceptedIntegrationEvent(
            TenantId: loan.TenantId,
            LoanApplicationId: loan.Id,
            GuarantorMemberId: guarantor.GuarantorMemberId,
            LockedAmount: guarantor.LockedAmount,
            CreatedBy: actorProvider.ActorId.ToString()
        );

        await publisher.Publish(guarantorAcceptedEvent, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogGuarantorPledgeAccepted(logger, guarantor.Id, loan.Id);

        return AppResponses.Success(true);
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Guarantor pledge {GuarantorId} was accepted for Loan {LoanId}.")]
    private static partial void LogGuarantorPledgeAccepted(ILogger logger, Guid guarantorId, Guid loanId);
}
