using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Events;

public sealed record PasswordResetSuccessEvent(
    string UserId,
    string Email,
    string FullName

) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
