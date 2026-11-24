using Asp.Versioning;
using LS.Api.Common.Authorization;
using LS.Api.Common.Controllers;
using LS.Application.Features.Membership.Commands;
using LS.Application.Features.Membership.Queries;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LS.Api.Features.Membership.Controllers;

/// <summary>
/// Handles member-related endpoints.
/// </summary>
[Route("api/v{version:apiVersion}/members")]
[ApiVersion("1.0")]
[ApiController]
public class MembersController(ISender sender) : BaseController
{
    /// <summary>
    /// Gets a specific member by ID.
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("members.view")]
    [ProducesResponseType(typeof(AppResponse<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMember(Guid id)
    {
        var query = new GetMemberQuery(id);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets all members.
    /// </summary>
    [HttpGet]
    [RequirePermission("members.view")]
    [ProducesResponseType(typeof(AppResponse<List<MemberResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers()
    {
        var query = new GetMembersQuery();
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets all members pending approval.
    /// </summary>
    [HttpGet("pending")]
    [RequirePermission("members.approve")]
    [ProducesResponseType(typeof(AppResponse<List<MemberResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingMembers()
    {
        var query = new GetPendingMembersQuery();
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets the dashboard statistics for the membership module.
    /// </summary>
    [HttpGet("dashboard-stats")]
    [RequirePermission("members.view")]
    [ProducesResponseType(typeof(AppResponse<MembershipDashboardStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var query = new GetMembershipDashboardStatsQuery();
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Onboards a new member (Maker step).
    /// </summary>
    [HttpPost("onboard")]
    [RequirePermission("members.create")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> OnboardMember([FromBody] OnboardMemberRequest request)
    {
        var command = new OnboardMemberCommand(request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Approves a pending member (Checker step).
    /// </summary>
    [HttpPost("approve")]
    [RequirePermission("members.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveMember([FromBody] ApproveMemberRequest request)
    {
        var command = new ApproveMemberCommand(request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Rejects a pending member (Checker step).
    /// </summary>
    [HttpPost("reject")]
    [RequirePermission("members.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectMember([FromBody] RejectMemberRequest request)
    {
        var command = new RejectMemberCommand(request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Triggers the KYC verification for a member.
    /// </summary>
    [HttpPost("{id}/verify-kyc")]
    [RequirePermission("members.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyMemberKyc(Guid id)
    {
        var command = new VerifyMemberKycCommand(id);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Activates the member's linked IAM account for portal sign-in.
    /// </summary>
    [HttpPost("{memberId:guid}/grant-portal-access")]
    [RequirePermission("members.approve")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GrantPortalAccess(Guid memberId)
    {
        var command = new GrantMemberPortalAccessCommand(memberId, GetActorId());
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Suspends a member.
    /// </summary>
    [HttpPost("{id}/suspend")]
    [RequirePermission("members.edit")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuspendMember(Guid id, [FromBody] string reason)
    {
        var command = new SuspendMemberCommand(id, reason);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Closes a member account.
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission("members.edit")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseMember(Guid id, [FromBody] string reason)
    {
        var command = new CloseMemberCommand(id, reason);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Gets a member's beneficiaries.
    /// </summary>
    [HttpGet("{id}/beneficiaries")]
    [RequirePermission("members.view")]
    [ProducesResponseType(typeof(AppResponse<List<BeneficiaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBeneficiaries(Guid id)
    {
        var query = new GetMemberBeneficiariesQuery(id);
        var result = await sender.Send(query).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Adds a beneficiary to a member.
    /// </summary>
    [HttpPost("{id}/beneficiaries")]
    [RequirePermission("members.edit")]
    [ProducesResponseType(typeof(AppResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddBeneficiary(Guid id, [FromBody] AddBeneficiaryRequest request)
    {
        var command = new AddBeneficiaryCommand(id, request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    /// <summary>
    /// Removes a beneficiary from a member.
    /// </summary>
    [HttpDelete("{id}/beneficiaries/{beneficiaryId}")]
    [RequirePermission("members.edit")]
    [ProducesResponseType(typeof(AppResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveBeneficiary(Guid id, Guid beneficiaryId)
    {
        var command = new RemoveBeneficiaryCommand(id, beneficiaryId);
        var result = await sender.Send(command).ConfigureAwait(false);
        return HandleResponse(result);
    }

    private string GetActorId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id was not found.");
        }

        return userId;
    }
}
