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
[Route("api/banking/fosa/accounts")]
[Authorize]
public class FosaAccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost("open")]
    public async Task<IActionResult> OpenAccount([FromBody] OpenFosaAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new OpenFosaAccountCommand(request.MemberId);
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("transactions/otc")]
    public async Task<IActionResult> ProcessOtcTransaction([FromBody] OtcTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<LS.Domain.Features.Banking.FOSA.Enums.OtcTransactionType>(request.TransactionType, true, out var transactionType))
        {
            return BadRequest(AppResponses.Failure<Guid>("Invalid Transaction Type."));
        }

        var command = new OverTheCounterTransactionCommand(
            request.TellerTillId,
            request.FosaAccountId,
            transactionType,
            request.Amount,
            request.Reference);
            
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
