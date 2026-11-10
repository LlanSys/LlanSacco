using LS.SharedKernel.Features.Shared.EmailTemplates.Enums;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.SharedKernel.Extensions;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;
using LS.Infrastructure.Logging;
using LS.Infrastructure.Utilities;
using LS.SharedKernel.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Infrastructure.Messaging.Consumers;


public abstract class EmailComposer<TEvent>(
    ICacheService _cache,
    ISharedUnitOfWork _sharedUnitOfWork,
    ILogger<EmailComposer<TEvent>> logger) : IEmailComposer<TEvent> where TEvent : IIntegrationEvent
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(1);

    public abstract Task<AppResponse<ComposeEmailResponse>> ComposeAsync(TEvent evt,CancellationToken ct);

    protected static ComposeEmailResponse ComposeFromTemplate(
        EmailTemplate template,
        string recipientName,
        string recipientEmail,
        IReadOnlyDictionary<string, string> tokens)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentNullException.ThrowIfNull(tokens);

        return new ComposeEmailResponse(
            template.Name,
            recipientName,
            recipientEmail,
            EmailTemplateRenderer.Render(template.Subject, tokens),
            EmailTemplateRenderer.Render(template.Body, tokens));
    }
        
    public async Task<EmailTemplate?> ResolveTemplateAsync(EmailTemplateType emailTemplateType, CancellationToken cancellationToken)
    {
        try
        {
            var key = CacheKeys.EmailTemplate(emailTemplateType.ToDisplayString());

            return await _cache.GetOrCreateAsync(
                key,
                async ct => await _sharedUnitOfWork.EmailTemplateRepository
                    .FindByCondition(t => t.Name == emailTemplateType.ToDisplayString())
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false),
                Ttl,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(logger, emailTemplateType.ToDisplayString(), ex);
            throw;
        }

    }

}


