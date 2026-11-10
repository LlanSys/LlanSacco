using LS.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.Shared.Notifications.Contracts.Interfaces;

public interface ISmsService
{
    Task<AppResponse<bool>> TwilioSendAsync(string phoneNumber, string message);
}
