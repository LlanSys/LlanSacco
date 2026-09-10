using LS.Api.Common.Controllers;
using LS.Application.Features.Dividends.Commands;
using LS.Application.Features.Dividends.Queries;
using LS.SharedKernel.Dtos.Dividends;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Api.Features.Dividends;

[Route("api/dividends")]
[Authorize]
public class DividendsController(IMediator mediator) : BaseController
{
    [HttpGet("declarations")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(typeof(AppResponse<IEnumerable<DividendDeclarationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeclarations()
    {
        var result = await mediator.Send(new GetDividendDeclarationsQuery());
        return HandleResponse(result);
    }

    [HttpGet("declarations/{declarationId:guid}")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(typeof(AppResponse<DividendDeclarationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeclarationById([FromRoute] Guid declarationId)
    {
        var result = await mediator.Send(new GetDividendDeclarationByIdQuery(declarationId));
        return HandleResponse(result);
    }

    [HttpGet("preferences")]
    [ProducesResponseType(typeof(AppResponse<IEnumerable<DividendPreferenceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences()
    {
        var result = await mediator.Send(new GetDividendPreferencesQuery());
        return HandleResponse(result);
    }
    [HttpPost("declare")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeclareDividends([FromBody] DeclareDividendsRequest request)
    {
        var command = new DeclareDividendsCommand(
            request.FinancialYear,
            request.ShareDividendRate,
            request.DepositInterestRate,
            request.ShareWhtRate,
            request.DepositWhtRate,
            request.Notes
        );

        var result = await mediator.Send(command);
        return HandleResponse(result);
    }

    [HttpPost("declarations/{declarationId:guid}/calculate")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunCalculation([FromRoute] Guid declarationId)
    {
        var command = new RunDividendCalculationCommand(declarationId);
        var result = await mediator.Send(command);
        return HandleResponse(result);
    }

    [HttpPost("declarations/{declarationId:guid}/approve")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveDeclaration([FromRoute] Guid declarationId)
    {
        var command = new ApproveDividendDeclarationCommand(declarationId);
        var result = await mediator.Send(command);
        return HandleResponse(result);
    }

    [HttpPost("declarations/{declarationId:guid}/distribute")]
    [Authorize(Policy = "ControlPlane.Manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostDistributions([FromRoute] Guid declarationId)
    {
        var command = new PostDividendDistributionsCommand(declarationId);
        var result = await mediator.Send(command);
        return HandleResponse(result);
    }

    [HttpPost("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPreferences([FromBody] SetDividendPreferenceRequest request)
    {
        var command = new SetDividendPreferenceCommand(
            request.CapitalizePercentage,
            request.FosaPercentage,
            request.ExternalBankPercentage
        );

        var result = await mediator.Send(command);
        return HandleResponse(result);
    }
}
