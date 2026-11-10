using LS.Domain.Features.ControlPlane.Auditing.Enums;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using LS.Application.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LS.SharedKernel.Features.ControlPlane.Auditing.Dtos;

namespace LS.Application.Features.ControlPlane.Auditing.Queries;

public record GetImpersonationRecordsQuery(bool ActiveOnly = false) : IRequest<AppResponse<IReadOnlyList<ImpersonationRecordResponse>>>, ICachableRequest
{
    public string CacheGroup => "auditing";
    public string Discriminator => $"impersonation_records_{ActiveOnly}";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

internal sealed class GetImpersonationRecordsQueryHandler(
    IControlPlaneUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider)
    : IRequestHandler<GetImpersonationRecordsQuery, AppResponse<IReadOnlyList<ImpersonationRecordResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<ImpersonationRecordResponse>>> Handle(GetImpersonationRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = await unitOfWork.ImpersonationRecords.ListAsync(
            q => request.ActiveOnly 
                ? q.Where(x => x.Status == ImpersonationRecordStatus.Active && x.ExpiryTime > DateTimeOffset.UtcNow)
                : q,
            cancellationToken).ConfigureAwait(false);

        var dtos = query.Select(x => new ImpersonationRecordResponse(
            x.Id,
            x.ActorId,
            x.ActorName,
            x.TargetTenantId,
            x.TargetTenantName,
            x.Reason,
            x.StartTime,
            x.ExpiryTime,
            x.Status.ToString()
        )).ToList();

        return AppResponses.Success<IReadOnlyList<ImpersonationRecordResponse>>(dtos);
    }
}

