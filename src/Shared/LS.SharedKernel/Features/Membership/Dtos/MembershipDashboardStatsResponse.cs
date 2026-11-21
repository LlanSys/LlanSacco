namespace LS.SharedKernel.Features.Membership.Dtos;

public record MembershipDashboardStatsResponse(
    int TotalMembers,
    int PendingApprovals,
    int ActiveMembers,
    int NewMembersThisMonth
);
