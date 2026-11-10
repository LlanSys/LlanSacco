using System.Collections.Generic;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Stamps.Queries;

public record GetAllDeploymentStampsQuery : IRequest<AppResponse<List<DeploymentStampResponse>>>, ICachableRequest
{
    public string CacheGroup => "stamps";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}
