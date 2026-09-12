using System;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Tenants.Queries;

public record GetTenantByIdQuery(Guid Id) : IRequest<AppResponse<TenantResponse>>, ICachableRequest
{
    public string CacheGroup => "tenants";
    public string Discriminator => Id.ToString();
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}
