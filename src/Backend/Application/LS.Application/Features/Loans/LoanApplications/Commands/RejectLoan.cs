using LS.Domain.Features.Loans.Contracts;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Loans.Guarantors;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public class RejectLoanCommand : IRequest<AppResponse<bool>>
{
    public Guid LoanApplicationId { get; set; }
    public required string Reason { get; set; }
}

internal sealed partial class RejectLoanCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    ILogger<RejectLoanCommandHandler> logger, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<RejectLoanCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(RejectLoanCommand request, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, cancellationToken);

                if (loan == null)
                {
                    return AppResponses.NotFound<bool>("Loan application not found.");
                }

                if (loan.Status != LoanStatus.Submitted)
                {
                    return AppResponses.Failure<bool>($"Cannot reject loan in status {loan.Status}.");
                }

                loan.Status = LoanStatus.Rejected;
                loan.UpdatedBy = actorProvider.ActorId.ToString();
                loan.UpdatedAt = clock.GetUtcNow();

                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, cancellationToken);
                await GuarantorReservationMapping.StageReleaseAsync(loan, unitOfWork, publisher, actorProvider.ActorId, clock.GetUtcNow(), cancellationToken).ConfigureAwait(false);

                LogLoanRejected(logger, loan.Id, loan.ApplicationNumber, request.Reason);

                return AppResponses.Success(true);
            },
            cancellationToken: cancellationToken);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Loan {ApplicationNumber} ({Id}) was rejected. Reason: {Reason}")]
    private static partial void LogLoanRejected(ILogger logger, Guid id, string applicationNumber, string reason);
}
