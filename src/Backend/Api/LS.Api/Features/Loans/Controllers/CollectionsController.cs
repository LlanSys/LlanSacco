using LS.Api.Common.Controllers;
using System;
using System.Threading.Tasks;
using LS.Application.Features.Loans.Collections.Commands;
using LS.Application.Features.Loans.Collections.Queries;
using LS.Domain.Features.Loans.Collections.Enums;
using LS.SharedKernel.Dtos.Loans.Collections;
using LS.Api.Common.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.Loans.Controllers;

[Route("api/v1/loans/collections")]
public class CollectionsController(ISender sender) : BaseController
{
    [HttpPost("cases")]
    [Authorize(Policy = "Permissions.Collections.Manage")]
    public async Task<IActionResult> OpenCollectionCase([FromBody] OpenCollectionCaseRequest request)
    {
        var command = new OpenCollectionCaseCommand(
            request.LoanApplicationId,
            request.TotalArrearsAmount,
            request.DaysPastDue,
            request.TriggeredBy
        );
        return HandleResponse(await sender.Send(command));
    }

    [HttpGet("cases")]
    [Authorize(Policy = "Permissions.Collections.View")]
    public async Task<IActionResult> GetActiveCases([FromQuery] Guid? officerUserId)
    {
        return HandleResponse(await sender.Send(new GetActiveCollectionCasesQuery(officerUserId)));
    }

    [HttpGet("cases/{id}")]
    [Authorize(Policy = "Permissions.Collections.View")]
    public async Task<IActionResult> GetCaseDetails(Guid id)
    {
        return HandleResponse(await sender.Send(new GetCollectionCaseDetailsQuery(id)));
    }

    [HttpPost("cases/{id}/assign-officer")]
    [Authorize(Policy = "Permissions.Collections.Manage")]
    public async Task<IActionResult> AssignOfficer(Guid id, [FromBody] AssignCollectionOfficerRequest request)
    {
        return HandleResponse(await sender.Send(new AssignCollectionOfficerCommand(id, request.OfficerUserId)));
    }

    [HttpPost("cases/{id}/actions")]
    [Authorize(Policy = "Permissions.Collections.Manage")]
    public async Task<IActionResult> RecordAction(Guid id, [FromBody] RecordCollectionActionRequest request)
    {
        if (!Enum.TryParse<CollectionActionType>(request.ActionType, true, out var actionTypeEnum))
            return BadRequest("Invalid action type");

        var command = new RecordCollectionActionCommand(
            id,
            actionTypeEnum,
            request.Notes,
            request.ActionDate
        );
        return HandleResponse(await sender.Send(command));
    }

    [HttpPost("cases/{id}/promises")]
    [Authorize(Policy = "Permissions.Collections.Manage")]
    public async Task<IActionResult> RecordPromise(Guid id, [FromBody] RecordPromiseToPayRequest request)
    {
        var command = new RecordPromiseToPayCommand(
            id,
            request.PromiseDate,
            request.PromiseAmount
        );
        return HandleResponse(await sender.Send(command));
    }

    [HttpPost("cases/{id}/resolve")]
    [Authorize(Policy = "Permissions.Collections.Manage")]
    public async Task<IActionResult> ResolveCase(Guid id, [FromBody] ResolveCollectionCaseRequest request)
    {
        return HandleResponse(await sender.Send(new ResolveCollectionCaseCommand(id, request.ResolutionReason)));
    }

    [HttpGet("promises/pending")]
    [Authorize(Policy = "Permissions.Collections.View")]
    public async Task<IActionResult> GetPendingPromises([FromQuery] Guid? officerUserId)
    {
        return HandleResponse(await sender.Send(new GetPendingPromisesQuery(officerUserId)));
    }
}


