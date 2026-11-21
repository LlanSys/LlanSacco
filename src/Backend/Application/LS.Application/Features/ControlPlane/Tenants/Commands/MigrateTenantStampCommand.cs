using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Events;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.Application.Contracts.Interfaces.Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.ControlPlane.Tenants.Commands;

public record MigrateTenantStampCommand(
    Guid TenantId,
    Guid NewDeploymentStampId,
    string NewDatabaseConnectionString) : IRequest<AppResponse<bool>>;


internal sealed partial class MigrateTenantStampCommandHandler(
    IControlPlaneUnitOfWork unitOfWork,
    IEncryptionService encryptionService,
    ILogger<MigrateTenantStampCommandHandler> logger) : IRequestHandler<MigrateTenantStampCommand, AppResponse<bool>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ILogger<MigrateTenantStampCommandHandler> _logger = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating Tenant {TenantId} to Stamp {StampId}.")]
    private partial void LogMigratingTenantStamp(Guid tenantId, Guid stampId);

    public async Task<AppResponse<bool>> Handle(MigrateTenantStampCommand request, CancellationToken cancellationToken)
    {
        LogMigratingTenantStamp(request.TenantId, request.NewDeploymentStampId);

        var tenant = await _unitOfWork.Tenants.FindByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant == null)
        {
            return AppResponses.Failure<bool>("Tenant not found.");
        }

        var oldStampId = tenant.DeploymentStampId;

        tenant.DeploymentStampId = request.NewDeploymentStampId;
        tenant.DatabaseConnectionString = _encryptionService.Encrypt(request.NewDatabaseConnectionString);

        // Raise domain event so that sessions/caches for this tenant can be invalidated
        tenant.RaiseDomainEvent(new TenantStampChangedDomainEvent(tenant.Id, oldStampId, request.NewDeploymentStampId));

        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<bool>("Tenant stamp migration completed successfully.", true);
    }
}
