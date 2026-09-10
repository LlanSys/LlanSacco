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
    Hangfire.IBackgroundJobClient backgroundJobClient,
    ILogger<PostCheckoffBatchCommandHandler> logger)
    : IRequestHandler<PostCheckoffBatchCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(PostCheckoffBatchCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var batch = await unitOfWork.CheckoffBatches.FirstOrDefaultAsync(
                b => b.Id == request.BatchId, cancellationToken);

            if (batch == null)
                return AppResponses.NotFound<bool>($"Batch with ID '{request.BatchId}' not found.");

            if (batch.Status != CheckoffBatchStatus.Validated)
                return AppResponses.Failure<bool>($"Batch is in status '{batch.Status}', cannot post. Batch must be Validated.");

            batch.Status = CheckoffBatchStatus.Posted;

            backgroundJobClient.Enqueue<LS.Application.Features.CheckOff.Jobs.MasterCheckoffBatchJob>(
                job => job.ExecuteAsync(request.BatchId, CancellationToken.None));

            return new AppResponse<bool>(true, "Batch posted successfully.");
        }, cancellationToken);
    }
}

