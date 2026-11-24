using Asp.Versioning;
using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Application.Features.Loans.LoanApplications.Commands;
using LS.Application.Features.Loans.LoanApplications.Queries;
using LS.Application.Features.Loans.LoanRepayments.Commands;
using LS.Application.Features.Loans.Guarantors.Commands;
using LS.Application.Features.Banking.GuarantorReservations.Queries;
using LS.Application.Features.Loans.Guarantors.Queries;
using LS.Application.Features.Membership.Queries;
using LS.SharedKernel.Dtos.Common;
using MediatR;
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
    [HttpGet("{id:guid}/guarantor-summary")]
    [RequirePermission("applications.view")]
    public async Task<IActionResult> GetGuarantorSummary(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new GetLoanGuarantorSummaryQuery(id), ct));

    [HttpGet("{id:guid}/guarantor-review-accounts/{pledgeId:guid}")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> GetReviewAccounts(Guid id, Guid pledgeId, CancellationToken ct) =>
        HandleResponse(await sender.Send(new GetGuarantorReviewAccountsQuery(id, pledgeId), ct));

    [HttpGet("guarantor-member-options")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> GetMemberOptions([FromQuery] string search, CancellationToken ct) =>
        HandleResponse(await sender.Send(new GetGuarantorMemberOptionsQuery(search), ct));

    [HttpGet("my-guarantor-pledges")]
    [MemberSelfService]
    public async Task<IActionResult> GetMyGuarantorPledges(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default) => HandleResponse(await sender.Send(new GetMyGuarantorPledgesQuery(page, pageSize), ct));

    [HttpGet("{id:guid}/guarantor-operations")]
    [RequirePermission("applications.view")]
    public async Task<IActionResult> GetGuarantorOperations(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new GetGuarantorOperationsQuery(id), ct));

    [HttpPost("guarantor-security-reviews/{id:guid}/cancel")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> CancelSecurityReview(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new CancelGuarantorSecurityReviewCommand(id), ct));

    [HttpPost("{id:guid}/guarantor-security-reviews")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> RequestSecurityReview(Guid id, GuarantorSecurityReviewRequest request, CancellationToken ct) =>
        HandleResponse(await sender.Send(
            new RequestGuarantorSecurityReviewCommand(
            request.ReviewId,
            id,
            request.PledgeId,
                request.Kind,
            request.EvidenceReference,
            request.SavingsAccountId,
            request.ReplacementMemberId),
            ct));

    [HttpPost("guarantor-security-reviews/{id:guid}/approve")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> ApproveSecurityReview(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new ApproveGuarantorSecurityReviewCommand(id), ct));

    [HttpGet("guarantor-reconciliation")]
    [RequirePermission("applications.view")]
    [RequirePermission("banking-accounts.view")]
    public async Task<IActionResult> GetGuarantorReconciliation(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) => HandleResponse(await sender.Send(new GetGuarantorReconciliationQuery(page, pageSize), ct));

    [HttpPost("{id:guid}/cancel")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason, CancellationToken ct) =>
        HandleResponse(await sender.Send(new CancelGuaranteedLoanCommand(id, reason), ct));

    [HttpPost("{id:guid}/guarantor-recoveries")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> RequestRecovery(Guid id, [FromBody] GuarantorRecoveryRequest request, CancellationToken ct) =>
        HandleResponse(await sender.Send(
            new RequestGuarantorRecoveryCommand(
            id,
            request.RecoveryId,
            request.Amount,
                request.EvidenceReference),
            ct));

    [HttpPost("guarantor-recoveries/{id:guid}/cancel")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> CancelRecovery(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new CancelGuarantorRecoveryCommand(id), ct));

    [HttpPost("guarantor-recoveries/{id:guid}/approve")]
    [RequirePermission("applications.approve")]
    public async Task<IActionResult> ApproveRecovery(Guid id, CancellationToken ct) =>
        HandleResponse(await sender.Send(new ApproveGuarantorRecoveryCommand(id), ct));

    [HttpPost]
    [RequirePermission("applications.create")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Apply([FromBody] ApplyForLoanCommand command)
    {
        var result = await sender.Send(command);
        return HandleResponse(result);
    }

    [HttpGet("/api/v{version:apiVersion}/loans/dashboard-stats")]
    [RequirePermission("applications.view")]
    [ProducesResponseType(typeof(AppResponse<LoanDashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await sender.Send(new GetLoanDashboardStatsQuery());
        return HandleResponse(result);
    }

    [HttpGet]
    [RequirePermission("applications.view")]
    [ProducesResponseType(typeof(AppResponse<List<LoanApplicationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApplications([FromQuery] Guid? memberId)
    {
        var result = await sender.Send(new GetLoanApplicationsQuery { MemberId = memberId });
        return HandleResponse(result);
    }

    [HttpPost("{id}/approve")]
    [RequirePermission("applications.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await sender.Send(new ApproveLoanCommand { LoanApplicationId = id });
        return HandleResponse(result);
    }

    [HttpPost("{id}/reject")]
    [RequirePermission("applications.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
    {
        var result = await sender.Send(new RejectLoanCommand { LoanApplicationId = id, Reason = reason });
        return HandleResponse(result);
    }

    [HttpPost("{id}/repayments")]
    [RequirePermission("repayments.create")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessRepayment(Guid id, [FromBody] ProcessRepaymentCommand command)
    {
        if (id != command.LoanApplicationId)
        {
            return BadRequest();
        }

        var result = await sender.Send(command);
        return HandleResponse(result);
    }

    [HttpPost("{id}/disburse")]
    [RequirePermission("applications.disburse")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Disburse(Guid id)
    {
        var result = await sender.Send(new DisburseLoanCommand(id));
        return HandleResponse(result);
    }

    [HttpPost("{id}/guarantors/accept")]
    [MemberSelfService]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptGuarantor(Guid id, [FromBody] AcceptGuarantorPledgeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new AcceptGuarantorPledgeCommand(
                id,
                request.GuarantorMemberId,
                request.AcceptedAmount,
                request.SavingsAccountId,
                request.TermsAccepted,
                request.TermsVersion,
                request.PledgeId
                ),
            ct);

        return HandleResponse(result);
    }
}
