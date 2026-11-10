using Asp.Versioning;
using LS.Api.Common.Controllers;
using LS.Application.Features.Membership.Commands;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Api.Features.Membership.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class GuarantorsController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpPost("requests")]
    public async Task<IActionResult> NominateGuarantor([FromBody] NominateGuarantorRequest request, CancellationToken cancellationToken)
    {
        var command = new NominateGuarantorCommand(request);
        var response = await _mediator.Send(command, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("requests/{id:guid}/respond")]
    public async Task<IActionResult> RespondToGuarantor(Guid id, [FromBody] RespondToGuarantorRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RequestId)
        {
            return BadRequest("Route ID must match Request ID");
        }
        
        // Retrieve logged-in user from context (we assume User.Identity.Name is populated via the IAM token).
        var currentUser = User.Identity?.Name ?? "Unknown";
        
        var command = new RespondToGuarantorCommand(request, currentUser);
        var response = await _mediator.Send(command, cancellationToken);
        return HandleResponse(response);
    }
}
