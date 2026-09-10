using Asp.Versioning;
using LS.Api.Common.Controllers;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Application.Features.Loans.LoanApplications.Commands;
using LS.Application.Features.Loans.LoanApplications.Queries;
using LS.Application.Features.Loans.LoanRepayments.Commands;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Api.Features.Loans;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/loans/applications")]
public class LoanApplicationsController(ISender sender) : BaseController
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Apply([FromBody] ApplyForLoanCommand command)
    {
        var result = await sender.Send(command);
        return HandleResponse(result);
    }

    [HttpGet("/api/v{version:apiVersion}/loans/dashboard-stats")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<LoanDashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await sender.Send(new GetLoanDashboardStatsQuery());
        return HandleResponse(result);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<List<LoanApplicationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApplications([FromQuery] Guid? memberId)
    {
        var result = await sender.Send(new GetLoanApplicationsQuery { MemberId = memberId });
        return HandleResponse(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await sender.Send(new ApproveLoanCommand { LoanApplicationId = id });
        return HandleResponse(result);
    }

    [HttpPost("{id}/reject")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
    {
        var result = await sender.Send(new RejectLoanCommand { LoanApplicationId = id, Reason = reason });
        return HandleResponse(result);
    }

    [HttpPost("{id}/repayments")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessRepayment(Guid id, [FromBody] ProcessRepaymentCommand command)
    {
        if (id != command.LoanApplicationId)
            return BadRequest();
        
        var result = await sender.Send(command);
        return HandleResponse(result);
    }
    [HttpPost("{id}/disburse")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Disburse(Guid id)
    {
        var result = await sender.Send(new DisburseLoanCommand(id));
        return HandleResponse(result);
    }

    [HttpPost("{id}/guarantors/accept")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptGuarantor(Guid id, [FromBody] AcceptGuarantorPledgeRequest request)
    {
        var result = await sender.Send(new AcceptGuarantorPledgeCommand(
            id,
            request.GuarantorMemberId,
            request.AcceptedAmount
        ));
        return HandleResponse(result);
    }
}

