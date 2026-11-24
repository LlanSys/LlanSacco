using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Common.Enums;
using NSubstitute;

namespace LS.Tests.Unit.Features.Membership;

public class GrantMemberPortalAccessCommandHandlerTests
{
    private const string Actor = "staff-user-id";
    private readonly IMembershipUnitOfWork _uow = Substitute.For<IMembershipUnitOfWork>();
    private readonly IMemberPortalAccessService _portalAccess = Substitute.For<IMemberPortalAccessService>();

    [Fact]
    public async Task Missing_member_is_not_found_and_iam_is_not_touched()
    {
        var result = await Handle(Guid.CreateVersion7());

        Assert.Equal(ErrorType.NotFound, result.Error?.Type);
        await AssertIamNotCalled();
    }

    [Theory]
    [InlineData(MemberStatus.PendingApproval, KycStatus.Verified, true)]
    [InlineData(MemberStatus.Suspended, KycStatus.Verified, true)]
    [InlineData(MemberStatus.Active, KycStatus.Pending, true)]
    [InlineData(MemberStatus.Active, KycStatus.Rejected, true)]
    [InlineData(MemberStatus.Active, KycStatus.Verified, false)]
    public async Task Ineligible_member_is_rejected_without_iam_changes(MemberStatus status, KycStatus kyc, bool linked)
    {
        var member = Member(status, kyc, linked ? Guid.CreateVersion7() : null);

        var result = await Handle(member.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BusinessRule, result.Error?.Type);
        await AssertIamNotCalled();
    }

    [Fact]
    public async Task Eligible_member_activates_the_linked_user_with_the_acting_staff_user()
    {
        var appUserId = Guid.CreateVersion7();
        var member = Member(MemberStatus.Active, KycStatus.Verified, appUserId);
        _portalAccess.GrantPortalAccessAsync(appUserId, member.Id, member.TenantId, Actor, Arg.Any<CancellationToken>())
            .Returns(AppResponses.Success("Portal access granted.", true));

        var result = await Handle(member.Id);

        Assert.True(result.IsSuccess);
        await _portalAccess.Received(1)
            .GrantPortalAccessAsync(appUserId, member.Id, member.TenantId, Actor, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Already_active_member_is_idempotent_and_iam_outcome_is_returned()
    {
        var appUserId = Guid.CreateVersion7();
        var member = Member(MemberStatus.Active, KycStatus.Verified, appUserId);
        _portalAccess.GrantPortalAccessAsync(appUserId, member.Id, member.TenantId, Actor, Arg.Any<CancellationToken>())
            .Returns(AppResponses.Success("Portal access is already active for this member.", true));

        var first = await Handle(member.Id);
        var second = await Handle(member.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal("Portal access is already active for this member.", second.Message);
    }

    [Fact]
    public async Task Iam_failure_is_returned_unchanged()
    {
        var appUserId = Guid.CreateVersion7();
        var member = Member(MemberStatus.Active, KycStatus.Verified, appUserId);
        _portalAccess.GrantPortalAccessAsync(appUserId, member.Id, member.TenantId, Actor, Arg.Any<CancellationToken>())
            .Returns(AppResponses.Failure<bool>(AppError.NotFound("The member's user account was not found.")));

        var result = await Handle(member.Id);

        Assert.Equal(ErrorType.NotFound, result.Error?.Type);
    }

    private Member Member(MemberStatus status, KycStatus kyc, Guid? appUserId)
    {
        var member = LS.Domain.Features.Membership.Entities.Member.Create(
            Guid.CreateVersion7(),
            "M-001",
            "Jane",
            "Doe",
            "jane@example.test",
            "+254700000000",
            "12345678",
            new DateOnly(1990, 1, 1),
            MembershipType.Individual,
            Actor);

        member.Status = status;
        member.KycStatus = kyc;
        member.AppUserId = appUserId;
        _uow.MemberRepository.FindByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        return member;
    }

    private Task<AppResponse<bool>> Handle(Guid memberId) =>
        new GrantMemberPortalAccessCommandHandler(_uow, _portalAccess)
            .Handle(new GrantMemberPortalAccessCommand(memberId, Actor), CancellationToken.None);

    private Task AssertIamNotCalled() =>
        _portalAccess.DidNotReceiveWithAnyArgs().GrantPortalAccessAsync(default, default, default, default!, default);
}
