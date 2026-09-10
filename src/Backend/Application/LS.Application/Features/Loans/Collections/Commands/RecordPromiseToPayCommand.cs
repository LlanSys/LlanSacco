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

public record RecordPromiseToPayCommand(
    Guid CollectionCaseId,
    DateTimeOffset PromiseDate,
    decimal PromiseAmount
) : IRequest<AppResponse<Guid>>;

internal class RecordPromiseToPayCommandHandler(
    ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ILogger<RecordPromiseToPayCommandHandler> logger
) : IRequestHandler<RecordPromiseToPayCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(RecordPromiseToPayCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;

        var collectionCase = await unitOfWork.CollectionCases.FindByIdAsync(request.CollectionCaseId, cancellationToken);
        if (collectionCase == null)
            return AppResponses.Failure<Guid>(AppError.BusinessRule("Collection case not found"));

        var promise = new CollectionPromise
        {
            TenantId = tenantId,
            CollectionCaseId = request.CollectionCaseId,
            PromiseDate = request.PromiseDate,
            PromiseAmount = request.PromiseAmount,
            Status = PromiseStatus.Pending,
            CreatedBy = "System"
        };

        await unitOfWork.CollectionPromises.CreateAsync(promise, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("Recorded pending promise {PromiseId} for case {CaseId}", promise.Id, request.CollectionCaseId);

        return AppResponses.Success<Guid>(promise.Id);
    }
}



