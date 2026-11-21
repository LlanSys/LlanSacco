using LS.SharedKernel.Features.Shared.EmailTemplates.Enums;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.SharedKernel.Extensions;
using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.IAM.Users.IntegrationEvents;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Entities;
using LS.Infrastructure.Logging;
using LS.Infrastructure.Messaging.Consumers;
using LS.Infrastructure.Utilities;
using LS.SharedKernel.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LS.Infrastructure.Features.HR.Employees.EmailComposers;


internal sealed class EmployeeWelcomeEmailComposer(
    ICacheService _cache,
    ISharedUnitOfWork _sharedUnitOfWork,
    ILogger<EmployeeWelcomeEmailComposer> _logger) : EmailComposer<EmployeeCreatedIntegrationEvent>(_cache, _sharedUnitOfWork, _logger)
{
    public override async Task<AppResponse<ComposeEmailResponse>> ComposeAsync(EmployeeCreatedIntegrationEvent evt, CancellationToken ct)
    {
        var templateType = EmailTemplateType.StandardWelcome;

        try
        {
            var template = await ResolveTemplateAsync(templateType, ct).ConfigureAwait(false);

            if (template == null)
                return AppResponses.Failure<ComposeEmailResponse>($"Template {templateType} not found");

            var tokens = new Dictionary<string, string>
            {
                ["EmployeeId"] = evt.EmployeeId.ToString(),
                ["EmployeeNumber"] = evt.Number,
                ["EmployeeName"] = evt.Name,
                ["Id"] = evt.EmployeeId.ToString(),
                ["Number"] = evt.Number,
                ["Name"] = evt.Name,
                ["Email"] = evt.Email,
                ["Date"] = evt.OccurredAt.ToString("f", CultureInfo.InvariantCulture)
            };

            return AppResponses.Success("Email composed", ComposeFromTemplate(
                template,
                evt.Name,
                evt.Email,
                tokens));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(_logger, templateType.ToString(), ex);
            throw;
        }
    }

}


