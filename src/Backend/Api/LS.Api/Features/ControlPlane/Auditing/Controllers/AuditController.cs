using Asp.Versioning;
using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.ControlPlane.Auditing.Commands;
using LS.Application.Features.ControlPlane.Auditing.Queries;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.ControlPlane.Auditing.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Api.Features.ControlPlane.Auditing.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/control-plane/audit")]
[ApiController]
[Authorize]
public class AuditController(ISender sender) : BaseController
{
    [HttpPost("impersonate/start")]
    [RequirePermission("control_plane.manage")]
    [ProducesResponseType(typeof(AppResponse<ImpersonationRecordResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartImpersonation(
        [FromBody] StartImpersonationRequest request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var command = new StartImpersonationCommand(request.TargetTenantId, request.Reason, request.DurationHours);
        return HandleResponse(await sender.Send(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("impersonate/end")]
    [RequirePermission("control_plane.manage")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EndImpersonation(
        [FromBody] EndImpersonationRequest request, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var command = new EndImpersonationCommand(request.ImpersonationRecordId);
        return HandleResponse(await sender.Send(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("impersonate/records")]
    [RequirePermission("control_plane.manage")]
    [ProducesResponseType(typeof(AppResponse<IReadOnlyList<ImpersonationRecordResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImpersonationRecords(
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        return HandleResponse(await sender.Send(new GetImpersonationRecordsQuery(activeOnly), cancellationToken).ConfigureAwait(false));
    }
}

