using LS.Api.Common.Controllers;
using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.Banking.Shares.Queries;
using LS.SharedKernel.Features.Banking.Shares.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.Banking.Shares.Controllers;

[ApiController]
[Route("api/v1/banking/shares")]
[Authorize]
public class SharesController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpGet("members/{memberId}")]
    public async Task<IActionResult> GetMemberShareAccounts(Guid memberId, CancellationToken cancellationToken)
    {
        var query = new GetMemberShareAccountsQuery(memberId);
        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResponse(result);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseShares([FromBody] PurchaseSharesRequest request, CancellationToken cancellationToken)
    {
        var command = new PurchaseSharesCommand(request.MemberId, request.ShareProductId, request.Amount, request.Notes);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResponse(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferShares([FromBody] TransferSharesRequest request, CancellationToken cancellationToken)
    {
        var command = new TransferSharesCommand(request.FromMemberId, request.ToMemberId, request.ShareProductId, request.NumberOfShares, request.Notes);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResponse(result);
    }
}
