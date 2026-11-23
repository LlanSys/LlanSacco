using LS.Domain.Features.Loans.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record AssignCollectionOfficerCommand(
    Guid CollectionCaseId,
    Guid OfficerUserId
) : IRequest<AppResponse<bool>>;

internal partial class AssignCollectionOfficerCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ILogger<AssignCollectionOfficerCommandHandler> logger
) : IRequestHandler<AssignCollectionOfficerCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(AssignCollectionOfficerCommand request, CancellationToken cancellationToken)
    {
        var collectionCase = await unitOfWork.CollectionCaseRepository.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.NotFound<bool>("Collection case not found");

        if (collectionCase.Status != Domain.Features.Loans.Collections.Enums.CollectionCaseStatus.Open)
            return AppResponses.Failure<bool>(AppError.BusinessRule($"Cannot assign officer to case in {collectionCase.Status} status"));

        collectionCase.AssignedOfficerUserId = request.OfficerUserId;

        await unitOfWork.CollectionCaseRepository.UpdateAsync(collectionCase, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogOfficerAssigned(logger, request.OfficerUserId, request.CollectionCaseId);

        return AppResponses.Success<bool>(true);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Assigned officer {OfficerId} to collection case {CaseId}")]
    private static partial void LogOfficerAssigned(ILogger logger, Guid officerId, Guid caseId);
}
