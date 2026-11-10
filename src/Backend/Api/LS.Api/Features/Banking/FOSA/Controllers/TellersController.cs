using System;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.Banking.FOSA.Commands;
using LS.SharedKernel.Dtos.Banking.FOSA;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.Banking.FOSA.Controllers;

[ApiController]
[Route("api/banking/fosa/tellers")]
[Authorize]
public class TellersController(IMediator mediator) : ControllerBase
{
    [HttpPost("open")]
    public async Task<IActionResult> OpenTill([FromBody] OpenTillRequest request, CancellationToken cancellationToken)
    {
        var command = new OpenTillCommand(request.TellerTillId, request.TellerUserId);
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseTill([FromBody] CloseTillRequest request, CancellationToken cancellationToken)
    {
        var breakdown = new LS.Domain.Features.Banking.FOSA.ValueObjects.DenominationBreakdown(
            request.Note1000Count, request.Note500Count, request.Note200Count, request.Note100Count, request.Note50Count,
            request.Coin20Count, request.Coin10Count, request.Coin5Count, request.Coin1Count);
        
        var command = new CloseTillCommand(request.TellerTillId, breakdown, request.Remarks);
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("vault-transfer")]
    public async Task<IActionResult> TransferToVault([FromBody] VaultTransferRequest request, CancellationToken cancellationToken)
    {
        var breakdown = new LS.Domain.Features.Banking.FOSA.ValueObjects.DenominationBreakdown(
            request.Note1000Count, request.Note500Count, request.Note200Count, request.Note100Count, request.Note50Count,
            request.Coin20Count, request.Coin10Count, request.Coin5Count, request.Coin1Count);
            
        var command = new VaultTransferCommand(request.TellerTillId, request.VaultId, breakdown, request.Remarks);
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
