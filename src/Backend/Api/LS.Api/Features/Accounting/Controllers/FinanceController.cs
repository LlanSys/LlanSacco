using Asp.Versioning;
using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.Accounting.Commands;
using LS.Application.Features.Accounting.Queries;
using LS.Application.Features.Accounting.Queries.GetIntegrationErrors;
using LS.Application.Features.Accounting.Queries.GetGlMappings;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LS.Api.Features.Accounting.Controllers;

/// <summary>
/// Handles financial reporting endpoints.
/// </summary>
[Route("api/v{version:apiVersion}/finance")]
[ApiVersion("1.0")]
[ApiController]
public class FinanceController(ISender sender) : BaseController
{
    /// <summary>
    /// Gets the trial balance for a specified date range.
    /// </summary>
    /// <param name="startDate">The start date (optional).</param>
    /// <param name="endDate">The end date (optional, defaults to now).</param>
    /// <returns>The trial balance.</returns>
    [HttpGet("trial-balance")]
    [RequirePermission("transactions.view")]
    [ProducesResponseType(typeof(AppResponse<TrialBalanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTimeOffset? asOfDate)
    {
        var query = new GetTrialBalanceQuery(asOfDate ?? DateTimeOffset.UtcNow);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets the dashboard statistics for the accounting module.
    /// </summary>
    [HttpGet("dashboard-stats")]
    [RequirePermission("transactions.view")]
    [ProducesResponseType(typeof(AppResponse<AccountingDashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await sender.Send(new GetAccountingDashboardStatsQuery()).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets the income statement for a specified date range.
    /// </summary>
    /// <param name="startDate">The start date (optional).</param>
    /// <param name="endDate">The end date (optional, defaults to now).</param>
    /// <returns>The income statement.</returns>
    [HttpGet("income-statement")]
    [RequirePermission("transactions.view")]
    [ProducesResponseType(typeof(AppResponse<IncomeStatementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var sDate = startDate ?? new DateTimeOffset(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var eDate = endDate ?? DateTimeOffset.UtcNow;
        var query = new GetIncomeStatementQuery(sDate, eDate);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets the balance sheet as of a specified date.
    /// </summary>
    /// <param name="asOfDate">The date to generate the balance sheet for (optional, defaults to now).</param>
    /// <returns>The balance sheet.</returns>
    [HttpGet("balance-sheet")]
    [RequirePermission("transactions.view")]
    [ProducesResponseType(typeof(AppResponse<BalanceSheetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTimeOffset? asOfDate)
    {
        var query = new GetBalanceSheetQuery(asOfDate ?? DateTimeOffset.UtcNow);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets the chart of accounts.
    /// </summary>
    /// <returns>A list of accounts.</returns>
    [HttpGet("accounts")]
    [RequirePermission("transactions.view")]
    [ProducesResponseType(typeof(AppResponse<System.Collections.Generic.IReadOnlyList<AccountResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts()
    {
        var query = new GetAccountsQuery();
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Posts a new journal entry.
    /// </summary>
    /// <param name="request">The journal entry details.</param>
    /// <returns>The ID of the posted journal.</returns>
    [HttpPost("journals")]
    [RequirePermission("transactions.create")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostJournal([FromBody] CreateJournalRequest request)
    {
        var command = new CreateJournalCommand(request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }
    /// <summary>
    /// Gets the list of integration errors (DLQ) for accounting events.
    /// </summary>
    /// <param name="status">Optional status filter (e.g., PendingRetry, Resolved).</param>
    /// <returns>A list of accounting integration errors.</returns>
    [HttpGet("integration-errors")]
    [RequirePermission("accounting-integrations.view")]
    [ProducesResponseType(typeof(AppResponse<System.Collections.Generic.IEnumerable<AccountingIntegrationErrorResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIntegrationErrors([FromQuery] string? status = null)
    {
        var query = new GetIntegrationErrorsQuery(status);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Retries a failed integration event from the DLQ.
    /// </summary>
    /// <param name="id">The ID of the integration error.</param>
    /// <returns>Success or failure of the retry operation.</returns>
    [HttpPost("integration-errors/{id}/retry")]
    [RequirePermission("accounting-integrations.retry")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryIntegrationError([FromRoute] Guid id)
    {
        var command = new RetryIntegrationEventCommand(id);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Lists the transaction-type GL mappings used for automated ledger posting.
    /// </summary>
    [HttpGet("gl-mappings")]
    [RequirePermission("gl-mappings.view")]
    [ProducesResponseType(typeof(AppResponse<System.Collections.Generic.IReadOnlyList<GlMappingResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlMappings()
    {
        var result = await sender.Send(new GetGlMappingsQuery()).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Creates or updates the GL mapping for a transaction type.
    /// </summary>
    /// <param name="transactionTypeCode">The transaction type code, for example GUARANTOR_RECOVERY.</param>
    /// <param name="request">The mapping sources and explicit ledger accounts.</param>
    [HttpPut("gl-mappings/{transactionTypeCode}")]
    [RequirePermission("gl-mappings.manage")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpsertGlMapping([FromRoute] string transactionTypeCode, [FromBody] UpsertGlMappingRequest request)
    {
        var result = await sender.Send(new UpsertGlMappingCommand(transactionTypeCode, request)).ConfigureAwait(false);
        return HandleResponse(result);
    }
}
