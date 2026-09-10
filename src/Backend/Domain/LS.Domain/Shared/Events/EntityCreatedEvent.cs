using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Shared.Events;

public sealed record EntityCreatedEvent() : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;

}

