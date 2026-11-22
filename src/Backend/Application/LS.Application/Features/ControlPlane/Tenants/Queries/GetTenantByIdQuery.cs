using System;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Tenants.Queries;

public record GetTenantByIdQuery(Guid Id) : IRequest<AppResponse<TenantResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Tenant details must reflect current provisioning and routing configuration.";
}
