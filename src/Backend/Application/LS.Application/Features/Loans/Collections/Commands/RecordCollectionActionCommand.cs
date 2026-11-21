using System;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Loans.Collections.Entities;
using LS.Domain.Features.Loans.Collections.Enums;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Loans.Collections.Commands;

public record RecordCollectionActionCommand(
    Guid CollectionCaseId,
    CollectionActionType ActionType,
    string Notes,
    DateTimeOffset? ActionDate = null
) : IRequest<AppResponse<Guid>>;

internal class RecordCollectionActionCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ILogger<RecordCollectionActionCommandHandler> logger
) : IRequestHandler<RecordCollectionActionCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(RecordCollectionActionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;

        var collectionCase = await unitOfWork.CollectionCases.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.Failure<Guid>(AppError.BusinessRule("Collection case not found"));

        var action = new CollectionAction
        {
            TenantId = tenantId,
            CollectionCaseId = request.CollectionCaseId,
            ActionType = request.ActionType,
            ActionDate = request.ActionDate ?? DateTimeOffset.UtcNow,
            Notes = request.Notes,
            CreatedBy = "System"
        };

        await unitOfWork.CollectionActions.CreateAsync(action, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("Recorded collection action {ActionId} for case {CaseId}", action.Id, request.CollectionCaseId);

        return AppResponses.Success<Guid>(action.Id);
    }
}



