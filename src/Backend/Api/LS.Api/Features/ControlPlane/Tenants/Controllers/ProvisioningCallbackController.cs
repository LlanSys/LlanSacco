using System;
using System.Threading.Tasks;
using LS.Application.Features.ControlPlane.Tenants.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LS.Api.Common.Authorization;
using LS.Api.Features.ControlPlane.Tenants.Dtos;
using Asp.Versioning;

namespace LS.Api.Features.ControlPlane.Tenants.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/controlplane/webhooks/provisioning-completed")]
public class ProvisioningCallbackController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("{tenantId:guid}")]
    [RequirePermission("control_plane.manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteProvisioning(
        [FromRoute] Guid tenantId, 
        [FromBody] CompleteTenantProvisioningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var command = new CompleteTenantProvisioningCommand(
            tenantId, 
            request.DatabaseConnectionString, 
            request.ApplicationInsightsKey);

        var result = await _mediator.Send(command).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
