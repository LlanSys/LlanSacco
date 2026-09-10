using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Specifications;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Commands.Batches;

public record ValidateCheckoffBatchCommand(Guid BatchId) : IRequest<AppResponse<bool>>;

internal class ValidateCheckoffBatchCommandValidator : AbstractValidator<ValidateCheckoffBatchCommand>
{
    public ValidateCheckoffBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}

internal sealed class ValidateCheckoffBatchCommandHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<ValidateCheckoffBatchCommandHandler> logger)
    : IRequestHandler<ValidateCheckoffBatchCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ValidateCheckoffBatchCommand request, CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var batch = await unitOfWork.CheckoffBatches.FirstOrDefaultAsync(
                b => b.Id == request.BatchId, cancellationToken);

            if (batch == null)
                return AppResponses.NotFound<bool>($"Batch with ID '{request.BatchId}' not found.");

            if (batch.Status != CheckoffBatchStatus.Staged)
                return AppResponses.Failure<bool>($"Batch is in status '{batch.Status}', cannot validate.");

            // Get staging rows
            var rows = await unitOfWork.CheckoffStagingRows.ListAsync(
                q => q.Where(r => r.CheckoffBatchId == request.BatchId), cancellationToken);

            int invalidCount = 0;

            foreach (var row in rows)
            {
                // Simple validation example
                var employment = await unitOfWork.MemberEmployments.FirstOrDefaultAsync(
                    me => me.EmployerId == batch.EmployerId && 
                          me.EmployeePayrollNumber == row.RawPayrollNumber && 
                          me.IsActive, cancellationToken);

                if (employment == null)
                {
                    row.Status = CheckoffRowStatus.Exception;
                    row.ExceptionReason = "Employee payroll number not found for this employer.";
                    invalidCount++;
                    continue;
                }
                
                // In a real system, you would cross-check against instructions to see if amount matches expectation
                // For simplicity, we just mark as valid if we found the employee link.
                row.Status = CheckoffRowStatus.Validated;
                row.ResolvedMemberId = employment.MemberId;
            }

            if (invalidCount == 0)
            {
                batch.Status = CheckoffBatchStatus.Validated;
            }
            else
            {
                batch.Status = CheckoffBatchStatus.Failed;
            }

            return new AppResponse<bool>(invalidCount == 0, $"Batch validation completed. {invalidCount} invalid rows.");
        }, cancellationToken);
    }
}

