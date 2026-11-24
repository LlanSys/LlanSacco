using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Middleware;

/// <summary>
/// Resolves the request tenant before anything reads it. A tenant host must map to exactly one Active
/// tenant on this deployment stamp; on a central API host the workspace selector header must name an
/// Active tenant on this stamp. Malformed or conflicting selectors are rejected instead of ignored.
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next, IOptions<OrgSettings> options)
{
    public const string ResolvedTenantItem = "ResolvedRequestTenant";

    public async Task InvokeAsync(HttpContext context, IControlPlaneUnitOfWork controlPlane)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(controlPlane);

        if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.StartsWithSegments("/ready", StringComparison.OrdinalIgnoreCase))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var settings = options.Value;
        var headers = context.Request.Headers;
        var host = context.Request.Host.Host.TrimEnd('.').ToLowerInvariant();
        var stampId = settings.DeploymentStampId;
        Guid? selectedTenant = null;

        if (headers.TryGetValue(settings.HeaderName, out var values))
        {
            if (values.Count != 1 || !Guid.TryParse(values[0], out var selector) || selector == Guid.Empty)
            {
                await Reject(context, StatusCodes.Status400BadRequest, "The workspace selector is invalid.").ConfigureAwait(false);
                return;
            }

            selectedTenant = selector;
        }

        if (!settings.CentralApiHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
        {
            var tenants = await controlPlane.TenantRepository.ListAsync(query => query
                .Where(item => item.HostName.ToLower() == host && (!stampId.HasValue || item.DeploymentStampId == stampId))
                .Take(2), context.RequestAborted).ConfigureAwait(false);

            if (tenants.Count != 1 || tenants[0].Status != TenantStatus.Active)
            {
                await Reject(context, StatusCodes.Status404NotFound, "This workspace host is unavailable.").ConfigureAwait(false);
                return;
            }

            if (selectedTenant is Guid supplied && supplied != tenants[0].Id)
            {
                await Reject(context, StatusCodes.Status400BadRequest, "The workspace selector conflicts with the request host.").ConfigureAwait(false);
                return;
            }

            selectedTenant = tenants[0].Id;
        }
        else if (selectedTenant is Guid supplied && !await controlPlane.TenantRepository.AnyAsync(
            item => item.Id == supplied && item.Status == TenantStatus.Active && (!stampId.HasValue || item.DeploymentStampId == stampId),
            context.RequestAborted).ConfigureAwait(false))
        {
            await Reject(context, StatusCodes.Status404NotFound, "This workspace is unavailable.").ConfigureAwait(false);
            return;
        }

        if (selectedTenant is Guid tenantId)
        {
            context.Items[ResolvedTenantItem] = tenantId;
            headers[settings.HeaderName] = tenantId.ToString();
        }

        await next(context).ConfigureAwait(false);
    }

    private static Task Reject(HttpContext context, int status, string title)
    {
        return Results.Problem(statusCode: status, title: title).ExecuteAsync(context);
    }
}
