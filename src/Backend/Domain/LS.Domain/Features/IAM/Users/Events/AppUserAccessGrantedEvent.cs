using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.IAM.Users.Events;


public sealed record AppUserAccessGrantedEvent(
    string Id,
    Guid? EmployeeId,
    Guid? MemberId,
    string GrantedBy

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
