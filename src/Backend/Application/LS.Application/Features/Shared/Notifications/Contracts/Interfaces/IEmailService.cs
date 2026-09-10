using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using LS.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.Shared.Notifications.Contracts.Interfaces;

public interface IEmailService
{
    Task<AppResponse<SendEmailResponse>> SendEmailAsync(SendEmailRequest sendEmailRequest, CancellationToken cancellationToken);
}
