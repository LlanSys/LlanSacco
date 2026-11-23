using LS.Application.Contracts.Interfaces.Common;
using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Queries.Batches;

public record GetCheckoffBatchDetailsQuery(Guid BatchId) : IRequest<AppResponse<CheckoffBatchDetailsResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Batch details include row errors and results updated by validation and posting.";
}

internal class GetCheckoffBatchDetailsQueryValidator : AbstractValidator<GetCheckoffBatchDetailsQuery>
{
    public GetCheckoffBatchDetailsQueryValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}

internal sealed class GetCheckoffBatchDetailsQueryHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<GetCheckoffBatchDetailsQueryHandler> logger)
    : IRequestHandler<GetCheckoffBatchDetailsQuery, AppResponse<CheckoffBatchDetailsResponse>>
{
    public async Task<AppResponse<CheckoffBatchDetailsResponse>> Handle(GetCheckoffBatchDetailsQuery request, CancellationToken cancellationToken)
    {
        var batch = await unitOfWork.CheckoffBatchRepository.FirstOrDefaultAsync(
            b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
            return AppResponses.NotFound<CheckoffBatchDetailsResponse>("Batch not found.");

        var employer = await unitOfWork.EmployerRepository.FirstOrDefaultAsync(
            e => e.Id == batch.EmployerId, cancellationToken);

        var rows = await unitOfWork.CheckoffStagingRowRepository.ListAsync(
            q => q.Where(r => r.CheckoffBatchId == batch.Id), cancellationToken);

        var rowIds = rows.Select(r => r.Id).ToList();

        var allocations = await unitOfWork.CheckoffStagingLoanAllocationRepository.ListAsync(
            q => q.Where(a => rowIds.Contains(a.CheckoffStagingRowId)), cancellationToken);

        var rowResponses = rows.Select(r => new CheckoffStagingRowResponse(
            r.Id,
            r.RawPayrollNumber,
            r.RawMemberName,
            r.AmountForSavings,
            r.AmountForShares,
            r.AmountForLoans,
            r.ResolvedMemberId,
            r.Status.ToString(),
            r.ExceptionReason,
            allocations.Where(a => a.CheckoffStagingRowId == r.Id)
                       .Select(a => new CheckoffStagingLoanAllocationResponse(a.Id, a.LoanId, a.Amount))
                       .ToList()
        )).ToList();

        var response = new CheckoffBatchDetailsResponse(
            new CheckoffBatchResponse(
                batch.Id,
                batch.EmployerId,
                employer?.Name ?? "Unknown",
                batch.BatchReference,
                batch.ProcessingPeriod,
                batch.TotalAmountReceived,
                batch.Status.ToString(),
                batch.CreatedAt
            ),
            rowResponses
        );

        return new AppResponse<CheckoffBatchDetailsResponse>(response, "Batch details retrieved successfully.");
    }
}

