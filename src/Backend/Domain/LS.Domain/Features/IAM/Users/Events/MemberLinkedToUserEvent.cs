using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.IAM.Users.Events;

public sealed record MemberLinkedToUserEvent(
    string UserId,
    Guid MemberId
) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
