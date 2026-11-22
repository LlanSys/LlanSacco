using System.Collections.Generic;
using System; using LS.SharedKernel.Dtos.Common; using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos; using MediatR;  namespace LS.Application.Features.ControlPlane.Tenants.Commands;  public record RemoveTenantModuleCommand(Guid TenantId, RemoveTenantModuleRequest Request) : IRequest<AppResponse<TenantResponse>>;
