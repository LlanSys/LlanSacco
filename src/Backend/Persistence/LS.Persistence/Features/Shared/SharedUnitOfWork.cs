using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Features.Shared.Payments.Contracts.Repositories;
using LS.Domain.Features.Shared.OrgSettings.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Logging;
using LS.Persistence.Features.Shared.DataContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Shared;

public sealed class SharedUnitOfWork(
    SharedDBContext context,
    IEmailTemplateRepository emailTemplateRepository,
    IFailedMessageRepository failedMessageRepository,
    IPaymentRecordRepository paymentRecordRepository,
    IOrgSettingRepository OrgSettingRepository,
    IPublisher publisher,
    ILogger<SharedUnitOfWork> logger
) : BaseUnitOfWork<SharedDBContext>(context, publisher, logger), ISharedUnitOfWork
{
    private readonly IPublisher _publisher = publisher;

    public IEmailTemplateRepository EmailTemplateRepository { get; } = emailTemplateRepository;
    public IFailedMessageRepository FailedMessageRepository { get; } = failedMessageRepository;
    public IPaymentRecordRepository PaymentRecordRepository { get; } = paymentRecordRepository;
    public IOrgSettingRepository OrgSettingRepository { get; } = OrgSettingRepository;

    public async Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default)
    {
        if (Context.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("CompleteWithEventsAsync must own the commit.");

        var count = await CompleteAsync(ct).ConfigureAwait(false);
        if (appEvents is not null)
            foreach (var appEvent in appEvents)
                await _publisher.Publish(appEvent, ct).ConfigureAwait(false);

        return count;
    }
}
