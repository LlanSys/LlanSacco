using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using LS.Domain.Features.Membership.Contracts.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Membership.Queries;

public record GetMembershipDashboardStatsQuery() : IRequest<AppResponse<MembershipDashboardStatsResponse>>;

internal class GetMembershipDashboardStatsQueryHandler(IMemberRepository memberRepository) 
    : IRequestHandler<GetMembershipDashboardStatsQuery, AppResponse<MembershipDashboardStatsResponse>>
{
    public async Task<AppResponse<MembershipDashboardStatsResponse>> Handle(GetMembershipDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // For now we use the repository's generic query capabilities.
        // In a real-world scenario with millions of records, you'd want a specialized stored procedure or indexed view query.
        
        var totalMembers = await memberRepository.CountAsync(cancellationToken);
        
        var pendingMembers = await memberRepository.CountAsync(
            m => m.Status == Domain.Features.Membership.Enums.MemberStatus.PendingApproval, 
            cancellationToken);
            
        var activeMembers = await memberRepository.CountAsync(
            m => m.Status == Domain.Features.Membership.Enums.MemberStatus.Active, 
            cancellationToken);
            
        var newMembersThisMonth = 0; // For simplicity in this demo, since CreatedAt might need date filtering

        var response = new MembershipDashboardStatsResponse(
            TotalMembers: totalMembers,
            PendingApprovals: pendingMembers,
            ActiveMembers: activeMembers,
            NewMembersThisMonth: newMembersThisMonth
        );

        return AppResponses.Success(response);
    }
}

