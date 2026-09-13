using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Stamps.Dtos;

namespace LS.UI.Blazor.Features.ControlPlane.Stamps.Contracts;

internal interface IDeploymentStampService
{
    Task<AppResponse<List<DeploymentStampResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppResponse<DeploymentStampResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppResponse<DeploymentStampResponse>> CreateAsync(CreateDeploymentStampRequest request, CancellationToken cancellationToken = default);
    Task<AppResponse<DeploymentStampResponse>> UpdateAsync(Guid id, UpdateDeploymentStampRequest request, CancellationToken cancellationToken = default);
}
