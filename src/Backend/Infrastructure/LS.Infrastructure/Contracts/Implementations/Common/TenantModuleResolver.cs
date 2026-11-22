using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LS.Infrastructure.Configuration;

namespace LS.Infrastructure.Contracts.Implementations.Common;

internal sealed class TenantModuleResolver(
    ICurrentTenantProvider tenantProvider,
    IServiceProvider serviceProvider,
    Microsoft.Extensions.Options.IOptions<LS.Infrastructure.Configuration.ShowcaseSettings> showcaseSettings) : ITenantModuleResolver
{
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ShowcaseSettings _showcaseSettings = showcaseSettings.Value;

    public async Task<IReadOnlyList<string>> GetEnabledModulesAsync(CancellationToken cancellationToken = default)
    {
        var globalModules = new List<string>();
        if (_showcaseSettings.EnableGlobalShowcase)
        {
            globalModules.Add("Showcase");
        }

        Guid tenantId;
        try
        {
            tenantId = _tenantProvider.TenantId;
        }
        catch (InvalidOperationException)
        {
            return globalModules;
        }

        if (tenantId == Guid.Empty)
        {
            return globalModules;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();

        var modules = await unitOfWork.TenantRepository.FindAll()
            .Where(t => t.Id == tenantId)
            .SelectMany(t => t.Modules)
            .Where(m => m.IsActive)
            .Select(m => m.ModuleKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return modules.Concat(globalModules).Distinct().ToList();
    }
}
