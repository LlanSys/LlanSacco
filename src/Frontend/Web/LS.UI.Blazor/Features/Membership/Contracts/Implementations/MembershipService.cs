using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using LS.UI.Blazor.Features.Membership.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Membership.Contracts.Implementations;

internal sealed class MembershipService(IBackendApiClient apiClient) : IMembershipService
{
    public Task<AppResponse<MemberResponse>> GetMemberAsync(Guid id)
    {
        return apiClient.SendAsync<MemberResponse>(
            HttpMethod.Get,
            $"api/v1.0/members/{id}");
    }

    public Task<AppResponse<MembershipDashboardStatsResponse>> GetDashboardStatsAsync()
    {
        return apiClient.SendAsync<MembershipDashboardStatsResponse>(
            HttpMethod.Get,
            "api/v1.0/members/dashboard-stats");
    }

    public Task<AppResponse<List<MemberResponse>>> GetPendingMembersAsync()
    {
        return apiClient.SendAsync<List<MemberResponse>>(
            HttpMethod.Get,
            "api/v1.0/members/pending");
    }

    public Task<AppResponse<List<MemberResponse>>> GetMembersAsync()
    {
        return apiClient.SendAsync<List<MemberResponse>>(
            HttpMethod.Get,
            "api/v1.0/members");
    }

    public Task<AppResponse<Guid>> OnboardMemberAsync(OnboardMemberRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/v1.0/members/onboard",
            request);
    }

    public Task<AppResponse<bool>> ApproveMemberAsync(ApproveMemberRequest request)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            "api/v1.0/members/approve",
            request);
    }

    public Task<AppResponse<bool>> RejectMemberAsync(RejectMemberRequest request)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            "api/v1.0/members/reject",
            request);
    }

    public Task<AppResponse<Guid>> NominateGuarantorAsync(NominateGuarantorRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/v1.0/guarantors/requests",
            request);
    }

    public Task<AppResponse<bool>> RespondToGuarantorAsync(RespondToGuarantorRequest request)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            $"api/v1.0/guarantors/requests/{request.RequestId}/respond",
            request);
    }

    public Task<AppResponse<bool>> VerifyMemberKycAsync(Guid memberId)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            $"api/v1.0/members/{memberId}/verify-kyc");
    }

    public Task<AppResponse<bool>> SuspendMemberAsync(Guid memberId, string reason)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            $"api/v1.0/members/{memberId}/suspend",
            reason);
    }

    public Task<AppResponse<bool>> CloseMemberAsync(Guid memberId, string reason)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            $"api/v1.0/members/{memberId}/close",
            reason);
    }

    public Task<AppResponse<List<BeneficiaryResponse>>> GetMemberBeneficiariesAsync(Guid memberId)
    {
        return apiClient.SendAsync<List<BeneficiaryResponse>>(
            HttpMethod.Get,
            $"api/v1.0/members/{memberId}/beneficiaries");
    }

    public Task<AppResponse<Guid>> AddBeneficiaryAsync(Guid memberId, AddBeneficiaryRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            $"api/v1.0/members/{memberId}/beneficiaries",
            request);
    }

    public Task<AppResponse<bool>> RemoveBeneficiaryAsync(Guid memberId, Guid beneficiaryId)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Delete,
            $"api/v1.0/members/{memberId}/beneficiaries/{beneficiaryId}");
    }
}


