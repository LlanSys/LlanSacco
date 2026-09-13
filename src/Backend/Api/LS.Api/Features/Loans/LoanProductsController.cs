using Asp.Versioning;
using LS.Api.Common.Controllers;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Application.Features.Loans.LoanProducts.Commands;
using LS.Application.Features.Loans.LoanProducts.Queries;
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
[Route("api/v{version:apiVersion}/loans/products")]
public class LoanProductsController(ISender sender) : BaseController
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateLoanProductCommand command)
    {
        var result = await sender.Send(command);
        return HandleResponse(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLoanProductCommand command)
    {
        if (id != command.Id)
            return BadRequest();
            
        var result = await sender.Send(command);
        return HandleResponse(result);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<List<LoanProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts()
    {
        var result = await sender.Send(new GetLoanProductsQuery());
        return HandleResponse(result);
    }
}

