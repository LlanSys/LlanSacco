using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.HR.Employees.Events;

public sealed record EmployeeCreatedEvent(
    Guid EmployeeId,
    string Number,
    string Email,
    string Name

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
