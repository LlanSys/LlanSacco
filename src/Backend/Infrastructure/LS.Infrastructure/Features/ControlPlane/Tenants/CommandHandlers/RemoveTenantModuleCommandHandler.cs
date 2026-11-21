using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.ControlPlane.Tenants.Commands;
using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using LS.SharedKernel.Extensions;
using LS.Infrastructure.Logging;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Features.ControlPlane.Tenants.CommandHandlers;

public class RemoveTenantModuleCommandHandler(
    IControlPlaneUnitOfWork unitOfWork,
    ILogger<RemoveTenantModuleCommandHandler> logger,
    ICurrentActorProvider actorProvider) : IRequestHandler<RemoveTenantModuleCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<RemoveTenantModuleCommandHandler> _logger = logger;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<TenantResponse>> Handle(RemoveTenantModuleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var tenant = await _unitOfWork.Tenants.FindAll()
            .Include(t => t.Modules)
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            return AppResponses.Failure<TenantResponse>("Tenant not found.");
        }

        var moduleKey = request.Request.ModuleKey.Trim();
        var existingModule = tenant.Modules.FirstOrDefault(m => string.Equals(m.ModuleKey, moduleKey, StringComparison.OrdinalIgnoreCase));

        if (existingModule != null && existingModule.IsActive)
        {
            existingModule.IsActive = false;
            existingModule.UpdatedAt = DateTimeOffset.UtcNow;
            existingModule.UpdatedBy = _actorProvider.ActorId;

            tenant.RaiseDomainEvent(new LS.Domain.Features.ControlPlane.Tenants.Events.TenantModuleRevokedDomainEvent(tenant.Id, moduleKey));

            await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            ControlPlaneLogDefinitions.LogTenantModuleRemoved(_logger, moduleKey, tenant.Id);
        }

        var dto = new TenantResponse
        {
            Id = tenant.Id,
            Identifier = tenant.Identifier,
            DisplayName = tenant.DisplayName,
            HostName = tenant.HostName,
            ContactEmail = tenant.ContactEmail,
            MaxUsers = tenant.MaxUsers,
            SubscriptionTier = tenant.SubscriptionTier.ToDisplayString(),
            Status = tenant.Status.ToDisplayString(),
            DeploymentStampId = tenant.DeploymentStampId,
            DatabaseProvider = tenant.DatabaseProvider,
            DatabaseConnectionString = tenant.DatabaseConnectionString != null ? "********" : null,
            EnabledModules = tenant.Modules.Where(m => m.IsActive).Select(m => m.ModuleKey).ToList(),
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };

        return AppResponses.Success(dto);
    }
}
