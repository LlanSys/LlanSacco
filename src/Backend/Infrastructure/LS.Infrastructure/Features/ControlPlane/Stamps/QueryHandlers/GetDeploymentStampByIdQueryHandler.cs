using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.ControlPlane.Stamps.Queries;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using LS.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Features.ControlPlane.Stamps.QueryHandlers;

public class GetDeploymentStampByIdQueryHandler(IControlPlaneUnitOfWork unitOfWork) : IRequestHandler<GetDeploymentStampByIdQuery, AppResponse<DeploymentStampResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<DeploymentStampResponse>> Handle(GetDeploymentStampByIdQuery request, CancellationToken cancellationToken)
    {
        var stamp = await _unitOfWork.DeploymentStamps
            .FindAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (stamp == null)
        {
            return AppResponses.Failure<DeploymentStampResponse>("Deployment stamp not found.");
        }

        var response = new DeploymentStampResponse
        {
            Id = stamp.Id,
            Name = stamp.Name,
            TargetResourceGroup = stamp.TargetResourceGroup,
            IsolationTier = stamp.IsolationTier.ToDisplayString(),
            KeyVaultUri = stamp.KeyVaultUri,
            DatabaseProvider = stamp.DatabaseProvider,
            DatabaseConnectionString = stamp.DatabaseConnectionString != null ? "********" : null,
            CreatedAt = stamp.CreatedAt,
            UpdatedAt = stamp.UpdatedAt
        };

        return AppResponses.Success(response);
    }
}
