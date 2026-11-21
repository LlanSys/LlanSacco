using Asp.Versioning;
using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.HR.Payroll.Commands;
using LS.Application.Features.HR.Payroll.Queries;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LS.Api.Features.HR.Payroll.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/payroll")]
[ApiController]
public sealed class PayrollController(ISender sender) : BaseController
{
    [HttpPost("run")]
    [RequirePermission("hr.payroll.run")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunPayroll([FromBody] RunPayrollRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new RunPayrollCommand(request.PayrollPeriodId), cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("/api/v{version:apiVersion}/hr/dashboard-stats")]
    [Authorize]
    [ProducesResponseType(typeof(AppResponse<HrDashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetHrDashboardStatsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("close-period")]
    [RequirePermission("hr.payroll.close")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClosePayrollPeriod([FromBody] ClosePayrollPeriodRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new ClosePayrollPeriodCommand(request.PayrollPeriodId), cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("periods")]
    [RequirePermission("hr.payroll.view")]
    [ProducesResponseType(typeof(AppResponse<IEnumerable<PayrollPeriodResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPayrollPeriods([FromQuery] GetPayrollPeriodsRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetPayrollPeriodsQuery(), cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("configuration")]
    [RequirePermission("hr.payroll.settings")]
    [ProducesResponseType(typeof(AppResponse<PayrollStatutoryConfigurationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConfiguration(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetPayrollConfigurationQuery(), cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpPost("configuration")]
    [RequirePermission("hr.payroll.settings")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateConfiguration([FromBody] UpdatePayrollConfigurationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePayrollConfigurationCommand(
            request.NssfTier1Limit,
            request.NssfTier2Limit,
            request.NssfRate,
            request.ShifRate,
            request.HousingLevyRate,
            request.PersonalReliefAmount,
            request.PayeTaxBands,
            User.Identity?.Name ?? "Admin"
        );
        var response = await sender.Send(command, cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }

    [HttpGet("periods/{id}/payslips")]
    [RequirePermission("hr.payroll.view")]
    [ProducesResponseType(typeof(AppResponse<IEnumerable<PayslipResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPayslips(Guid id, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetPayslipsByPeriodQuery(id), cancellationToken);
        if (response.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
}

