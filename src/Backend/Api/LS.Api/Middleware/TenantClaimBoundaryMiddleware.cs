using System.Security.Claims;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Middleware;
using Microsoft.Extensions.Options;

namespace LS.Api.Middleware;

/// <summary>
/// A signed-in session may only act in the workspace it was issued for, and only while that workspace is
/// Active on this deployment stamp. Runs after authentication, before any authorization or handler.
/// </summary>
internal sealed class TenantClaimBoundaryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IOptions<OrgSettings> settings, IControlPlaneUnitOfWork unitOfWork)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.Items.TryGetValue(TenantResolutionMiddleware.ResolvedTenantItem, out var resolved)
            && resolved is Guid selected
            && (!Guid.TryParse(context.User.FindFirstValue("tenant_id"), out var signedTenant) || signedTenant != selected))
        {
            await Results.Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "The account session belongs to a different workspace.").ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true
            && settings.Value.DeploymentStampId is Guid stampId
            && (!Guid.TryParse(context.User.FindFirstValue("tenant_id"), out var tenantId)
                || !await unitOfWork.TenantRepository.AnyAsync(
                    tenant => tenant.Id == tenantId && tenant.DeploymentStampId == stampId && tenant.Status == TenantStatus.Active,
                    context.RequestAborted).ConfigureAwait(false)))
        {
            await Results.Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "This account session is unavailable on this deployment.").ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        await next(context).ConfigureAwait(false);
    }
}
