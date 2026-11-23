namespace LS.Application.Features.Membership.Contracts.Interfaces;

public interface ICurrentMemberResolver
{
    Task<Guid?> FindActiveMemberIdAsync(CancellationToken cancellationToken);
}
