using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.ControlPlane.Tenants.Queries;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using LS.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Features.ControlPlane.Tenants.QueryHandlers;

public class GetAllTenantsQueryHandler(IControlPlaneUnitOfWork unitOfWork) : IRequestHandler<GetAllTenantsQuery, AppResponse<List<TenantResponse>>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;

    public async Task<AppResponse<List<TenantResponse>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var rawTenants = await _unitOfWork.Tenants.FindAll()
            .Include(t => t.Modules)
            .AsNoTracking()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var tenants = rawTenants
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Identifier = t.Identifier,
                DisplayName = t.DisplayName,
                HostName = t.HostName,
                ContactEmail = t.ContactEmail,
                MaxUsers = t.MaxUsers,
                SubscriptionTier = t.SubscriptionTier.ToDisplayString(),
                Status = t.Status.ToDisplayString(),
                DeploymentStampId = t.DeploymentStampId,
                DatabaseProvider = t.DatabaseProvider,
                DatabaseConnectionString = t.DatabaseConnectionString != null ? "********" : null,
                EnabledModules = t.Modules?.Where(m => m.IsActive).Select(m => m.ModuleKey).ToList() ?? [],
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToList();

        return AppResponses.Success(tenants);
    }
}
