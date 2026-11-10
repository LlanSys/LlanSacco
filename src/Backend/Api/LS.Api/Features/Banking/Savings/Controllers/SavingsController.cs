using LS.Api.Common.Controllers;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Application.Features.Banking.Savings.Queries;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Api.Features.Banking.Savings.Controllers;

[Authorize]
[Route("api/banking/savings")]
[ApiController]
public class SavingsController(IMediator mediator) : BaseController
{
    [HttpGet("members/{memberId}")]
    public async Task<IActionResult> GetMemberAccounts(Guid memberId)
    {
        var response = await mediator.Send(new GetMemberSavingsAccountsQuery(memberId)).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositSavingsRequest request)
    {
        var response = await mediator.Send(new DepositSavingsCommand(request)).ConfigureAwait(false);
        return HandleResponse(response);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(WithdrawSavingsRequest request)
    {
        var response = await mediator.Send(new WithdrawSavingsCommand(request)).ConfigureAwait(false);
        return HandleResponse(response);
    }
}
