using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.IAM.Users.IntegrationEvents;

internal sealed record AppUserCreatedIntegrationEvent(
    string UserId,
    Guid TenantId,
    Guid? EmployeeId,
    string UserName,
    string FullName,
    string Email

) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

