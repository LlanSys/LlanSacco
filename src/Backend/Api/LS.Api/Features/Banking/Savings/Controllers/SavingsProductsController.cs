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
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool onlyActive = true)
    {
        var response = await mediator.Send(new GetSavingsProductsQuery(onlyActive));
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSavingsProductRequest request)
    {
        var response = await mediator.Send(new CreateSavingsProductCommand(request));
        return HandleResponse(response);
    }
}
