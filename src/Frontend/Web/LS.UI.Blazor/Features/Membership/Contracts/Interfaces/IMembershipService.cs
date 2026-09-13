using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Membership.Contracts.Interfaces;

internal interface IMembershipService
{
    Task<AppResponse<MemberResponse>> GetMemberAsync(Guid id);
    Task<AppResponse<MembershipDashboardStatsResponse>> GetDashboardStatsAsync();
    Task<AppResponse<List<MemberResponse>>> GetPendingMembersAsync();
    Task<AppResponse<List<MemberResponse>>> GetMembersAsync();
    Task<AppResponse<Guid>> OnboardMemberAsync(OnboardMemberRequest request);
    Task<AppResponse<bool>> ApproveMemberAsync(ApproveMemberRequest request);
    Task<AppResponse<bool>> RejectMemberAsync(RejectMemberRequest request);
    Task<AppResponse<Guid>> NominateGuarantorAsync(NominateGuarantorRequest request);
    Task<AppResponse<bool>> RespondToGuarantorAsync(RespondToGuarantorRequest request);
    
    // New endpoints
    Task<AppResponse<bool>> VerifyMemberKycAsync(Guid memberId);
    Task<AppResponse<bool>> SuspendMemberAsync(Guid memberId, string reason);
    Task<AppResponse<bool>> CloseMemberAsync(Guid memberId, string reason);
    Task<AppResponse<List<BeneficiaryResponse>>> GetMemberBeneficiariesAsync(Guid memberId);
    Task<AppResponse<Guid>> AddBeneficiaryAsync(Guid memberId, AddBeneficiaryRequest request);
    Task<AppResponse<bool>> RemoveBeneficiaryAsync(Guid memberId, Guid beneficiaryId);
}


