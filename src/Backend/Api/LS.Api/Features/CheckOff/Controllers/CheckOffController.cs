using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.Application.Features.CheckOff.Commands.Instructions;
using LS.Application.Features.CheckOff.Queries.Batches;
using LS.Application.Features.CheckOff.Queries.Employers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.CheckOff.Controllers;

[Route("api/checkoff")]
[Authorize]
public class CheckOffController(IMediator mediator) : BaseController
{
    [HttpGet("employers")]
    [RequirePermission("checkoff.view")]
    public async Task<IActionResult> GetEmployers()
    {
        var query = new GetEmployersQuery();
        var response = await mediator.Send(query, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpGet("batches")]
    [RequirePermission("checkoff.view")]
    public async Task<IActionResult> GetBatches()
    {
        var query = new GetCheckoffBatchesQuery();
        var response = await mediator.Send(query, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpGet("batches/{batchId:guid}")]
    [RequirePermission("checkoff.view")]
    public async Task<IActionResult> GetBatchDetails(Guid batchId)
    {
        var query = new GetCheckoffBatchDetailsQuery(batchId);
        var response = await mediator.Send(query, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("instructions")]
    [RequirePermission("checkoff.prepare")]
    public async Task<IActionResult> CreateInstruction([FromBody] CreateCheckoffInstructionCommand command)
    {
        var response = await mediator.Send(command, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("batches/upload")]
    [RequirePermission("checkoff.prepare")]
    public async Task<IActionResult> UploadBatch([FromBody] UploadCheckoffBatchCommand command)
    {
        var response = await mediator.Send(command, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("batches/{batchId:guid}/validate")]
    [RequirePermission("checkoff.prepare")]
    public async Task<IActionResult> ValidateBatch(Guid batchId)
    {
        var command = new ValidateCheckoffBatchCommand(batchId);
        var response = await mediator.Send(command, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("batches/{batchId:guid}/post")]
    [RequirePermission("checkoff.post")]
    public async Task<IActionResult> PostBatch(Guid batchId)
    {
        var command = new PostCheckoffBatchCommand(batchId);
        var response = await mediator.Send(command, HttpContext.RequestAborted).ConfigureAwait(false);
        return HandleResponse(response);
    }
}
