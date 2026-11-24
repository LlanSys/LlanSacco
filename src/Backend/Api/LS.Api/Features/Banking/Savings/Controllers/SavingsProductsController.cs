using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Application.Features.Banking.Savings.Queries;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Api.Features.Banking.Savings.Controllers;

[Authorize]
[Route("api/banking/savings-products")]
public class SavingsProductsController(IMediator mediator) : BaseController
{
    [HttpPut("{id:guid}/security-policy")]
    [RequirePermission("banking-products.manage")]
    public async Task<IActionResult> ConfigureSecurity(Guid id, ConfigureSavingsSecurityRequest request, CancellationToken ct) =>
        HandleResponse(await mediator.Send(new ConfigureSavingsSecurityCommand(id, request), ct));

    [HttpGet]
    [RequirePermission("banking-products.view")]
    public async Task<IActionResult> Get([FromQuery] bool onlyActive = true)
    {
        var response = await mediator.Send(new GetSavingsProductsQuery(onlyActive));
        return HandleResponse(response);
    }

    [HttpPost]
    [RequirePermission("banking-products.manage")]
    public async Task<IActionResult> Create(CreateSavingsProductRequest request)
    {
        var response = await mediator.Send(new CreateSavingsProductCommand(request));
        return HandleResponse(response);
    }
}
