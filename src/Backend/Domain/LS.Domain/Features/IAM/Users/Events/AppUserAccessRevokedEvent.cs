using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.IAM.Users.Events;


public sealed record AppUserAccessRevokedEvent(
    string Id,
    Guid? EmployeeId,
    Guid? MemberId,
    string RevokedBy,
    string RevokeReason

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
