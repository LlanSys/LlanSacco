using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Common;
using System.Collections.ObjectModel;
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
    ICurrentTenantProvider tenantProvider)
    : IRequestHandler<ValidateCheckoffBatchCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ValidateCheckoffBatchCommand request, CancellationToken cancellationToken)
    {
        if (tenantProvider.TenantId == Guid.Empty) return AppResponses.Failure<bool>(AppError.Forbidden("A tenant context is required."));
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var batch = await unitOfWork.CheckoffBatches.FirstOrDefaultAsync(
                b => b.Id == request.BatchId && b.TenantId == tenantProvider.TenantId, cancellationToken);

            if (batch == null)
                return AppResponses.NotFound<bool>("Batch not found.");

            if (batch.Status != CheckoffBatchStatus.Staged)
                return AppResponses.Failure<bool>($"Batch is in status '{batch.Status}', cannot validate.");

            var rows = await unitOfWork.CheckoffStagingRows.ListAsync(
                q => q.Where(r => r.CheckoffBatchId == batch.Id && r.TenantId == tenantProvider.TenantId), cancellationToken);
            if (rows.Count == 0) return AppResponses.Failure<bool>("An empty batch cannot be validated.");

            // Payroll identifiers are opaque, case-sensitive values. Never choose an arbitrary match.
            var payrollNumbers = rows.Select(r => r.RawPayrollNumber).Distinct(StringComparer.Ordinal).ToArray();
            var employments = new List<MemberEmployment>();
            foreach (var chunk in payrollNumbers.Chunk(500))
            {
                employments.AddRange(await unitOfWork.MemberEmployments.ListAsync(q => q.Where(e =>
                    e.TenantId == tenantProvider.TenantId && e.EmployerId == batch.EmployerId && e.IsActive &&
                    chunk.Contains(e.EmployeePayrollNumber)), cancellationToken));
            }
            var matches = employments.ToLookup(e => e.EmployeePayrollNumber, StringComparer.Ordinal);
            var duplicates = rows.GroupBy(r => r.RawPayrollNumber, StringComparer.Ordinal)
                .Where(g => g.Skip(1).Any()).Select(g => g.Key).ToHashSet(StringComparer.Ordinal);
            var balanced = rows.Sum(r => r.TotalDeducted) == batch.TotalAmountReceived;
            var invalidCount = 0;
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                row.ResolvedMemberId = null;
                row.ExceptionReason = null;
                var candidates = matches[row.RawPayrollNumber].ToArray();
                if (!balanced) row.ExceptionReason = "Batch total does not match the staged deductions.";
                else if (duplicates.Contains(row.RawPayrollNumber)) row.ExceptionReason = "Duplicate payroll number in this batch.";
                else if (row.AmountForSavings < 0 || row.AmountForShares < 0 || row.AmountForLoans < 0 ||
                    row.TotalDeducted <= 0 || row.TotalDeducted != row.AmountForSavings + row.AmountForShares + row.AmountForLoans)
                    row.ExceptionReason = "Deduction amounts are invalid or do not balance.";
                else if (candidates.Length == 0) row.ExceptionReason = "Employee payroll number not found for this employer.";
                else if (candidates.Length != 1) row.ExceptionReason = "Multiple active employments match this payroll number.";
                else row.ResolvedMemberId = candidates[0].MemberId;
                row.Status = row.ExceptionReason is null ? CheckoffRowStatus.Validated : CheckoffRowStatus.Exception;
                if (row.ExceptionReason is not null) invalidCount++;
            }
            batch.Status = invalidCount == 0 ? CheckoffBatchStatus.Validated : CheckoffBatchStatus.Failed;
            await unitOfWork.CheckoffStagingRows.UpdateRangeAsync(new Collection<CheckoffStagingRow>(rows), cancellationToken);
            await unitOfWork.CheckoffBatches.UpdateAsync(batch, cancellationToken);
            // Validation itself succeeded; Data reports validity and the persisted row diagnostics explain failures.
            return new AppResponse<bool>(invalidCount == 0, $"Batch validation completed. {invalidCount} invalid rows.");
        }, cancellationToken);
    }
}
