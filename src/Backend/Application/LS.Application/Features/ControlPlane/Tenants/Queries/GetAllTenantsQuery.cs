using System;
using System.Collections.Generic;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Tenants.Queries;

public record GetAllTenantsQuery : IRequest<AppResponse<List<TenantResponse>>>, ICachableRequest
{
    public string CacheGroup => "tenants";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}
