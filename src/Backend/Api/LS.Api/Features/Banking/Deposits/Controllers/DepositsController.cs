using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.Banking.Deposits.Commands;
using LS.Application.Features.Banking.Deposits.Queries;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Api.Features.Banking.Deposits.Controllers;

public class DepositsController(IMediator mediator) : BaseController
{
    [HttpPost("products")]
    [RequirePermission("banking-products.manage")]
    public async Task<IActionResult> CreateProduct(CreateDepositProductRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateDepositProductCommand(request), cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("products")]
    [RequirePermission("banking-products.view")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetDepositProductsQuery(), cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("accounts")]
    [RequirePermission("banking-accounts.open")]
    public async Task<IActionResult> OpenAccount(OpenDepositAccountRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new OpenDepositAccountCommand(request), cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("accounts/member/{memberId}")]
    [RequirePermission("banking-accounts.view")]
    public async Task<IActionResult> GetMemberAccounts(Guid memberId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetMemberDepositAccountsQuery(memberId), cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("accounts/{accountId}/deposit")]
    [RequirePermission("banking-transactions.post")]
    public async Task<IActionResult> Deposit(Guid accountId, DepositTransactionRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new DepositToAccountCommand(accountId, request), cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("accounts/{accountId}/withdraw")]
    [RequirePermission("banking-transactions.post")]
    public async Task<IActionResult> Withdraw(Guid accountId, DepositTransactionRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new WithdrawFromDepositCommand(accountId, request), cancellationToken);
        return HandleResponse(response);
    }
}
