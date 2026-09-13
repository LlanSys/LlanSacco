using LS.Api.Common.Controllers;
using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.Banking.Shares.Queries;
using LS.SharedKernel.Features.Banking.Shares.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.Banking.Shares.Controllers;

[ApiController]
[Route("api/v1/banking/share-products")]
[Authorize]
public class ShareProductsController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetShareProducts([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = new GetShareProductsQuery(includeInactive);
        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateShareProduct([FromBody] CreateShareProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateShareProductCommand(request.Name, request.Code, request.Description ?? string.Empty, request.PricePerShare, request.MinimumShares);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResponse(result);
    }
}
