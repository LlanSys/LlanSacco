using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.CheckOff.IntegrationEvents;
using LS.Domain.Shared.Contracts.Common;
using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace LS.Application.Features.CheckOff.Commands.Batches;

public record PostCheckoffBatchCommand(Guid BatchId) : IRequest<AppResponse<bool>>;

internal class PostCheckoffBatchCommandValidator : AbstractValidator<PostCheckoffBatchCommand>
{
    public PostCheckoffBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}

internal sealed class PostCheckoffBatchCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    IContextEventPublisher<ICheckOffUnitOfWork> publisher,
    ICurrentTenantProvider tenantProvider, ICurrentActorProvider actorProvider)
    : IRequestHandler<PostCheckoffBatchCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(PostCheckoffBatchCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) return AppResponses.Failure<bool>(AppError.Forbidden("A tenant is required."));
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var batch = await unitOfWork.CheckoffBatches.FirstOrDefaultAsync(
                b => b.TenantId == tenantId && b.Id == request.BatchId, cancellationToken);

            if (batch == null)
                return AppResponses.NotFound<bool>($"Batch with ID '{request.BatchId}' not found.");

            if (batch.Status is CheckoffBatchStatus.Posting or CheckoffBatchStatus.Posted)
                return AppResponses.Success("Batch posting has already been accepted.", true);
            if (batch.Status != CheckoffBatchStatus.Validated)
                return AppResponses.Failure<bool>($"Batch is in status '{batch.Status}', cannot post. Batch must be Validated.");

            batch.Status = CheckoffBatchStatus.Posting;
            await unitOfWork.CheckoffBatches.UpdateAsync(batch, cancellationToken);
            await publisher.PublishAsync(new CheckoffPostingRequested(tenantId, batch.Id, actorProvider.ActorId), cancellationToken);
            return AppResponses.Success("Batch posting accepted.", true);
        }, cancellationToken);
    }
}

