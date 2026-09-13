using FluentValidation;
using LS.Domain.Features.CheckOff.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Queries.Batches;

public record GetCheckoffBatchesQuery : IRequest<AppResponse<List<CheckoffBatchResponse>>>;

internal sealed class GetCheckoffBatchesQueryHandler(
    ICheckOffUnitOfWork unitOfWork,
    ILogger<GetCheckoffBatchesQueryHandler> logger)
    : IRequestHandler<GetCheckoffBatchesQuery, AppResponse<List<CheckoffBatchResponse>>>
{
    public async Task<AppResponse<List<CheckoffBatchResponse>>> Handle(GetCheckoffBatchesQuery request, CancellationToken cancellationToken)
    {
        var batches = await unitOfWork.CheckoffBatches.ListAsync(
            q => q.OrderByDescending(b => b.CreatedAt),
            cancellationToken);

        var employerIds = batches.Select(b => b.EmployerId).Distinct().ToList();
        
        var employers = await unitOfWork.Employers.ListAsync(
            q => q.Where(e => employerIds.Contains(e.Id)),
            cancellationToken);

        var response = batches.Select(b => 
        {
            var employer = employers.FirstOrDefault(e => e.Id == b.EmployerId);
            return new CheckoffBatchResponse(
                b.Id,
                b.EmployerId,
                employer?.Name ?? "Unknown",
                b.BatchReference,
                b.ProcessingPeriod,
                b.TotalAmountReceived,
                b.Status.ToString(),
                b.CreatedAt
            );
        }).ToList();

        return new AppResponse<List<CheckoffBatchResponse>>(response, "Batches retrieved successfully.");
    }
}
