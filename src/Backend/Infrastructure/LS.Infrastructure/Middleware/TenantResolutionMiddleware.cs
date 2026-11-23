using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var settings = serviceProvider.GetRequiredService<IOptions<OrgSettings>>().Value;
        if (!context.Request.Headers.ContainsKey(settings.HeaderName))
        {
            var host = context.Request.Host.Host;
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
            var tenant = await unitOfWork.TenantRepository.FirstOrDefaultAsync(
                candidate => candidate.HostName == host && candidate.Status == TenantStatus.Active,
                context.RequestAborted).ConfigureAwait(false);

            if (tenant is not null && tenant.Id != Guid.Empty)
            {
                context.Request.Headers[settings.HeaderName] = tenant.Id.ToString();
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
