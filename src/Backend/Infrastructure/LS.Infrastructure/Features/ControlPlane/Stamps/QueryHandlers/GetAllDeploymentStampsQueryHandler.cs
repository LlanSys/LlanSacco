using System.Collections.Generic;
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

public class GetAllDeploymentStampsQueryHandler(IControlPlaneUnitOfWork unitOfWork) : IRequestHandler<GetAllDeploymentStampsQuery, AppResponse<List<DeploymentStampResponse>>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<DeploymentStampResponse>>> Handle(GetAllDeploymentStampsQuery request, CancellationToken cancellationToken)
    {
        var rawStamps = await _unitOfWork.DeploymentStamps.FindAll()
            .AsNoTracking()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var stamps = rawStamps
            .Select(s => new DeploymentStampResponse
            {
                Id = s.Id,
                Name = s.Name,
                TargetResourceGroup = s.TargetResourceGroup,
                IsolationTier = s.IsolationTier.ToDisplayString(),
                KeyVaultUri = s.KeyVaultUri,
                DatabaseProvider = s.DatabaseProvider,
                DatabaseConnectionString = s.DatabaseConnectionString != null ? "********" : null,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToList();

        return AppResponses.Success(stamps);
    }
}
