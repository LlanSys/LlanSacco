using LS.SharedKernel.Dtos.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

internal sealed partial class ApproveLoanCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    IPublisher publisher,
    ILogger<ApproveLoanCommandHandler> logger)
    : IRequestHandler<ApproveLoanCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken).ConfigureAwait(false);
        if (loan == null)
        {
            return AppResponses.Failure<bool>("Loan application not found.");
        }

        if (loan.Status != LoanStatus.Submitted)
        {
            return AppResponses.Failure<bool>("Cannot approve loan in status $($loan.Status).");
        }

        loan.Status = LoanStatus.Approved;
        loan.UpdatedBy = actorProvider.ActorId.ToString();
        loan.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogLoanApproved(logger, loan.Id, loan.ApplicationNumber);

        return AppResponses.Success(true);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan {ApplicationNumber} ({Id}) was approved.")]
    private static partial void LogLoanApproved(ILogger logger, Guid id, string applicationNumber);
}
