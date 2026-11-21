using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;

namespace LS.UI.Blazor.Features.ControlPlane.Tenants.Contracts;

internal interface ITenantService
{
    Task<AppResponse<List<TenantResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> SuspendAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> ApproveKYCAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> AddModuleAsync(Guid id, AddTenantModuleRequest request, CancellationToken cancellationToken = default);
    Task<AppResponse<TenantResponse>> RemoveModuleAsync(Guid id, string moduleKey, CancellationToken cancellationToken = default);
}
