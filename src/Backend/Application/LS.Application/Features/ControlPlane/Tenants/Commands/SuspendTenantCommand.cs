using LS.Application.Contracts.Interfaces.Common;
using System.Collections.Generic;
using System; using LS.SharedKernel.Dtos.Common; using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos; using MediatR;  namespace LS.Application.Features.ControlPlane.Tenants.Commands;  public record SuspendTenantCommand(Guid Id) : IRequest<AppResponse<TenantResponse>>, ICacheInvalidatorRequest
{
    public IEnumerable<string> CacheGroups => ["tenants"];
    public string? CacheUserId => null;
}
