using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Events;

public sealed record EmailOtpRequestedEvent(
    string UserId,
    string Email,
    string FirstName,
    string Code,
    string Purpose,
    DateTimeOffset ExpiresAt
) : INotification;
