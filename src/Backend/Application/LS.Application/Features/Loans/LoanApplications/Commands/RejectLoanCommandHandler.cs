using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

internal sealed partial class RejectLoanCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    ILogger<RejectLoanCommandHandler> logger)
    : IRequestHandler<RejectLoanCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken);
        if (loan == null)
        {
            return AppResponses.Failure<bool>("Loan application not found.");
        }

        if (loan.Status != LoanStatus.Submitted)
        {
            return AppResponses.Failure<bool>($"Cannot reject loan in status {loan.Status}.");
        }

        loan.Status = LoanStatus.Rejected;
        loan.UpdatedBy = actorProvider.ActorId.ToString();
        loan.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogLoanRejected(logger, loan.Id, loan.ApplicationNumber, request.Reason);

        return AppResponses.Success(true);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan {ApplicationNumber} ({Id}) was rejected. Reason: {Reason}")]
    private static partial void LogLoanRejected(ILogger logger, Guid id, string applicationNumber, string reason);
}

