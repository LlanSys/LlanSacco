using LS.Application.Contracts.Interfaces.Common;
using System.Collections.Generic;
using LS.SharedKernel.Dtos.Common; using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos; using MediatR;  using LS.Application.Contracts.Interfaces.Common; using System.Collections.Generic;  namespace LS.Application.Features.ControlPlane.Tenants.Commands;  public record CreateTenantCommand(CreateTenantRequest Request) : IRequest<AppResponse<TenantResponse>>, ICacheInvalidatorRequest {     public IEnumerable<string> CacheGroups => ["tenants"];     public string? CacheUserId => null; }
