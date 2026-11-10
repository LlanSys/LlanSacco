using LS.SharedKernel.Features.Shared.EmailTemplates.Enums;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.IAM.Users.IntegrationEvents;
using LS.Domain.Features.HR.Employees.Events;
using LS.Domain.Features.IAM.Users.Events;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;
using LS.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.Shared.Notifications.Contracts.Interfaces;

public interface IEmailComposer<TEvent> where TEvent : IIntegrationEvent
{
    Task<AppResponse<ComposeEmailResponse>> ComposeAsync(TEvent evt, CancellationToken ct);
    Task<EmailTemplate?> ResolveTemplateAsync(EmailTemplateType emailTemplateType, CancellationToken cancellationToken);

}


