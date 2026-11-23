using System;
using System.Collections.Generic;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Tenants.Queries;

public record GetAllTenantsQuery : IRequest<AppResponse<List<TenantResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Tenant administration must observe current provisioning, routing and access state.";
}
