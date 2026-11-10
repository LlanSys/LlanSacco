using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record AssignCollectionOfficerCommand(
    Guid CollectionCaseId,
    Guid OfficerUserId
) : IRequest<AppResponse<bool>>;

internal class AssignCollectionOfficerCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ILogger<AssignCollectionOfficerCommandHandler> logger
) : IRequestHandler<AssignCollectionOfficerCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(AssignCollectionOfficerCommand request, CancellationToken cancellationToken)
    {
        var collectionCase = await unitOfWork.CollectionCases.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.Failure<bool>(AppError.BusinessRule("Collection case not found"));

        if (collectionCase.Status != Domain.Features.Loans.Collections.Enums.CollectionCaseStatus.Open)
            return AppResponses.Failure<bool>(AppError.BusinessRule($"Cannot assign officer to case in {collectionCase.Status} status"));

        collectionCase.AssignedOfficerUserId = request.OfficerUserId;

        await unitOfWork.CollectionCases.UpdateAsync(collectionCase, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("Assigned officer {OfficerId} to collection case {CaseId}", request.OfficerUserId, request.CollectionCaseId);

        return AppResponses.Success<bool>(true);
    }
}



