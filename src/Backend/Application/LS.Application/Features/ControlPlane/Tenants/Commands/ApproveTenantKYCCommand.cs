using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.ControlPlane.Tenants.Commands;

public record ApproveTenantKYCCommand(Guid TenantId) : IRequest<AppResponse<TenantResponse>>;
