using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Contracts.Implementations.Common;

internal sealed class TenantConnectionResolver(
    ICurrentTenantProvider tenantProvider,
    IServiceProvider serviceProvider,
    IEncryptionService encryptionService) : ITenantConnectionResolver
{
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private TenantDatabaseMetadata? _metadata;

    public async Task<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        var tenantMetadata = await GetTenantMetadataAsync(cancellationToken).ConfigureAwait(false);
        return tenantMetadata?.ConnectionString;
    }

    public async Task<string?> GetDatabaseProviderAsync(CancellationToken cancellationToken = default)
    {
        var tenantMetadata = await GetTenantMetadataAsync(cancellationToken).ConfigureAwait(false);
        return tenantMetadata?.Provider;
    }

    private async Task<TenantDatabaseMetadata?> GetTenantMetadataAsync(CancellationToken cancellationToken)
    {
        Guid tenantId;
        try
        {
            tenantId = _tenantProvider.TenantId;
        }
        catch (TenantNotResolvedException)
        {
            // No tenant resolved for this request — pooled/anonymous path; return null so
            // TenantConnectionInterceptor leaves the default connection string untouched.
            return null;
        }

        // A scope may open several EF connections. Other requests must resolve
        // the latest placement and connection configuration.
        if (_metadata is { } current && current.TenantId == tenantId)
        {
            return current;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();

        var tenant = await unitOfWork.TenantRepository.FindByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant == null)
        {
            return null;
        }

        string? resolvedConnectionString = null;
        string? resolvedProvider = null;

        if (!string.IsNullOrEmpty(tenant.DatabaseConnectionString))
        {
            resolvedConnectionString = _encryptionService.Decrypt(tenant.DatabaseConnectionString);
            resolvedProvider = tenant.DatabaseProvider;
        }
        else
        {
            var stamp = await unitOfWork.DeploymentStampRepository.FindByIdAsync(tenant.DeploymentStampId, cancellationToken).ConfigureAwait(false);
            if (stamp != null && !string.IsNullOrEmpty(stamp.DatabaseConnectionString))
            {
                resolvedConnectionString = _encryptionService.Decrypt(stamp.DatabaseConnectionString);
                resolvedProvider = stamp.DatabaseProvider;
            }
        }

        var metadata = new TenantDatabaseMetadata(tenantId, resolvedConnectionString, resolvedProvider);
        _metadata = metadata;
        return metadata;
    }

    private sealed record TenantDatabaseMetadata(Guid TenantId, string? ConnectionString, string? Provider);
}
