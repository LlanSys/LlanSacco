using System;
using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace LS.Application.Features.ControlPlane.Stamps.Queries;

public record GetDeploymentStampByIdQuery(Guid Id) : IRequest<AppResponse<DeploymentStampResponse>>, ICachableRequest
{
    public string CacheGroup => "stamps";
    public string Discriminator => Id.ToString();
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}
