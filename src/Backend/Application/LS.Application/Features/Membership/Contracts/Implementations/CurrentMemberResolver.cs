using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Application.Features.Membership.Contracts.Implementations;

internal sealed class CurrentMemberResolver(
    IMembershipUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider) : ICurrentMemberResolver
{
    public Task<Guid?> FindActiveMemberIdAsync(CancellationToken cancellationToken) => FindAsync(false, cancellationToken);

    public Task<Guid?> FindVerifiedActiveMemberIdAsync(CancellationToken cancellationToken) => FindAsync(true, cancellationToken);

    private Task<Guid?> FindAsync(bool requireVerified, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;

        if (tenantId == Guid.Empty || !Guid.TryParse(actorProvider.ActorId, out var userId) || userId == Guid.Empty)
        {
            return Task.FromResult<Guid?>(null);
        }

        return unitOfWork.MemberRepository.FirstOrDefaultAsync(
            query => query
                .Where(member => member.TenantId == tenantId && member.AppUserId == userId && member.Status == MemberStatus.Active &&
                    (!requireVerified || member.KycStatus == KycStatus.Verified))
                .Select(member => (Guid?)member.Id),
            cancellationToken);
    }
}
