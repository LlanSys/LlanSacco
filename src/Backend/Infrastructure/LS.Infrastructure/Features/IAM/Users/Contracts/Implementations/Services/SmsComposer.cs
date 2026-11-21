using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Domain.Features.IAM.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

internal sealed class SmsComposer : ISmsComposer
{
    public SmsComposer()
    {

    }

    public async Task<string> ComposePasswordResetSmsAsync(RequestPasswordResetEvent evt)
    {
        var message = $"Dear {evt.FirstName}, your SACCO security code is {evt.ValidationCode}. " +
                      $"Valid for 10 mins. Do not share this code.";

        return await Task.FromResult(message).ConfigureAwait(false);
    }
}
