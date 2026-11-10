using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Application.Contracts.Interfaces.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.ControlPlane.Tenants.Commands;
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

public class UpdateTenantCommandHandler(
    IControlPlaneUnitOfWork unitOfWork,
    ILogger<UpdateTenantCommandHandler> logger,
    IEncryptionService encryptionService,
    ICurrentActorProvider actorProvider) : IRequestHandler<UpdateTenantCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UpdateTenantCommandHandler> _logger = logger;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ICurrentActorProvider _actorProvider = actorProvider;

    public async Task<AppResponse<TenantResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var tenant = await _unitOfWork.Tenants.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (tenant == null)
        {
            return AppResponses.Failure<TenantResponse>("Tenant not found.");
        }

        var duplicateHostName = await _unitOfWork.Tenants.AnyAsync(t => t.HostName == request.Request.HostName && t.Id != request.Id, cancellationToken).ConfigureAwait(false);

        if (duplicateHostName)
        {
            return AppResponses.Failure<TenantResponse>("Another tenant with this hostname already exists.");
        }

        SubscriptionTier parsedTier;
        try
        {
            parsedTier = request.Request.SubscriptionTier.ToEnum<SubscriptionTier>();
        }
        catch (ArgumentException)
        {
            return AppResponses.Failure<TenantResponse>("Invalid Subscription Tier.");
        }

        tenant.DisplayName = request.Request.DisplayName;
        tenant.HostName = request.Request.HostName;
        tenant.ContactEmail = request.Request.ContactEmail;
        tenant.MaxUsers = request.Request.MaxUsers;
        tenant.SubscriptionTier = parsedTier;
        
        if (request.Request.DatabaseProvider != null)
        {
            tenant.DatabaseProvider = request.Request.DatabaseProvider;
        }

        if (request.Request.DatabaseConnectionString != null)
        {
            tenant.DatabaseConnectionString = request.Request.DatabaseConnectionString != "" 
                ? _encryptionService.Encrypt(request.Request.DatabaseConnectionString) 
                : null;
        }

        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = _actorProvider.ActorId;

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        ControlPlaneLogDefinitions.LogTenantUpdated(_logger, tenant.Id, tenant.Identifier);

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
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
        return AppResponses.Success(dto);
    }
}



