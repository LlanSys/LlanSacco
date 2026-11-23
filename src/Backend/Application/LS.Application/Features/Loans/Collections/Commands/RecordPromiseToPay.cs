using LS.Domain.Features.Loans.Collections.Entities;
using LS.Domain.Features.Loans.Collections.Enums;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.Collections.Commands;

public record RecordPromiseToPayCommand(
    Guid CollectionCaseId,
    DateTimeOffset PromiseDate,
    decimal PromiseAmount
) : IRequest<AppResponse<Guid>>;

internal partial class RecordPromiseToPayCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ILogger<RecordPromiseToPayCommandHandler> logger
) : IRequestHandler<RecordPromiseToPayCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(RecordPromiseToPayCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;

        var collectionCase = await unitOfWork.CollectionCaseRepository.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.NotFound<Guid>("Collection case not found");

        var promise = new CollectionPromise
        {
            TenantId = tenantId,
            CollectionCaseId = request.CollectionCaseId,
            PromiseDate = request.PromiseDate,
            PromiseAmount = request.PromiseAmount,
            Status = PromiseStatus.Pending,
            CreatedBy = "System"
        };

        await unitOfWork.CollectionPromiseRepository.CreateAsync(promise, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        LogPromiseRecorded(logger, promise.Id, request.CollectionCaseId);

        return AppResponses.Success<Guid>(promise.Id);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Recorded pending promise {PromiseId} for case {CaseId}")]
    private static partial void LogPromiseRecorded(ILogger logger, Guid promiseId, Guid caseId);
}
